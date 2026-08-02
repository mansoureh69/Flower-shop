using SweetFlowerShop.Domain.Common;
using SweetFlowerShop.Domain.Enums;
using SweetFlowerShop.Domain.Events;
using SweetFlowerShop.Domain.Exceptions;
using SweetFlowerShop.Domain.ValueObjects;

namespace SweetFlowerShop.Domain.Entities;

/// <summary>
/// Historical purchase and fulfilment record.
/// Payment is a separate aggregate and may confirm an order only after provider verification.
/// </summary>
public class Order : AggregateRoot, IAuditable
{
    private readonly List<OrderItem> _items = new();

    public Guid CustomerId { get; private set; }
    public DateTime OrderDate { get; private set; }
    public OrderStatus Status { get; private set; }
    public DeliveryInfo DeliveryInfo { get; private set; } = null!;
    public string? Notes { get; private set; }

    public IReadOnlyCollection<OrderItem> Items => _items;
    public decimal TotalAmount => _items.Sum(item => item.TotalPrice);

    private Order() { }

    /// <summary>
    /// Creates a complete historical order from product and delivery snapshots.
    /// A placed order is eligible for payment but is not confirmed until payment succeeds.
    /// </summary>
    public static Order Place(
        Guid customerId,
        DeliveryInfo deliveryInfo,
        IReadOnlyCollection<OrderLineSnapshot> items,
        string? notes = null)
    {
        if (customerId == Guid.Empty)
            throw new ArgumentException("Customer ID is required.", nameof(customerId));

        ArgumentNullException.ThrowIfNull(deliveryInfo);
        ArgumentNullException.ThrowIfNull(items);

        if (items.Count == 0)
            throw new EmptyOrderException();

        if (items.Select(item => item.UnitPrice.Currency).Distinct().Count() != 1)
            throw new ArgumentException("All order lines must use the same currency.", nameof(items));

        var order = new Order
        {
            CustomerId = customerId,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.PendingPayment,
            DeliveryInfo = deliveryInfo,
            Notes = notes
        };

        foreach (var item in items)
            order.AddSnapshot(item);

        order.RaiseDomainEvent(
            new OrderPlacedEvent(order.Id, order.CustomerId, order.TotalAmount));

        return order;
    }

    public void SetDeliveryInfo(DeliveryInfo deliveryInfo)
    {
        EnsureStatus(OrderStatus.PendingPayment, "change delivery information for");
        ArgumentNullException.ThrowIfNull(deliveryInfo);
        DeliveryInfo = deliveryInfo;
    }

    /// <summary>
    /// Called after a successful payment has been verified.
    /// </summary>
    public void ConfirmPayment()
    {
        EnsureStatus(OrderStatus.PendingPayment, "confirm payment for");
        Status = OrderStatus.Confirmed;
        RaiseDomainEvent(new OrderConfirmedEvent(Id, CustomerId));
    }

    public void StartPreparing()
    {
        EnsureStatus(OrderStatus.Confirmed, "start preparing");
        Status = OrderStatus.Preparing;
    }

    public void MarkReadyForDelivery()
    {
        EnsureStatus(OrderStatus.Preparing, "mark ready for delivery");
        Status = OrderStatus.ReadyForDelivery;
    }

    public void MarkOutForDelivery()
    {
        EnsureStatus(OrderStatus.ReadyForDelivery, "send out for delivery");
        Status = OrderStatus.OutForDelivery;
    }

    public void MarkDelivered()
    {
        EnsureStatus(OrderStatus.OutForDelivery, "mark delivered");
        Status = OrderStatus.Delivered;
    }

    public void CancelUnpaid(string reason)
    {
        EnsureStatus(OrderStatus.PendingPayment, "cancel as unpaid");
        CancelCore(reason);
    }

    /// <summary>
    /// Called only after the paid-order refund policy has completed.
    /// </summary>
    public void CancelAfterRefund(string reason)
    {
        if (Status is not (OrderStatus.Confirmed
            or OrderStatus.Preparing
            or OrderStatus.ReadyForDelivery
            or OrderStatus.OutForDelivery))
        {
            throw new InvalidOrderStateException("cancel after refund", Status.ToString());
        }

        CancelCore(reason);
    }

    private void AddSnapshot(OrderLineSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        var existing = _items.FirstOrDefault(item =>
            item.ProductId == snapshot.ProductId
            && item.ProductName == snapshot.ProductName
            && item.UnitPrice == snapshot.UnitPrice
            && item.Notes == snapshot.Notes);

        if (existing is not null)
        {
            existing.UpdateQuantity(existing.Quantity + snapshot.Quantity);
            return;
        }

        _items.Add(new OrderItem(
            Id,
            snapshot.ProductId,
            snapshot.ProductName,
            snapshot.UnitPrice,
            snapshot.Quantity,
            snapshot.Notes));
    }

    private void CancelCore(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Cancellation reason is required.", nameof(reason));

        Status = OrderStatus.Cancelled;
        RaiseDomainEvent(new OrderCancelledEvent(Id, reason));
    }

    private void EnsureStatus(OrderStatus expectedStatus, string action)
    {
        if (Status != expectedStatus)
            throw new InvalidOrderStateException(action, Status.ToString());
    }
}
