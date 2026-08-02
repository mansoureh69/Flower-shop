using SweetFlowerShop.Domain.Common;
using SweetFlowerShop.Domain.Enums;
using SweetFlowerShop.Domain.Events;
using SweetFlowerShop.Domain.Exceptions;
using SweetFlowerShop.Domain.ValueObjects;

namespace SweetFlowerShop.Domain.Entities;

/// <summary>
/// Aggregate Root - Order represents a confirmed purchase intent.
/// Dependent entities: OrderItem
/// Payment is a SEPARATE aggregate — Order does not track PaymentStatus.
/// All item prices are snapshotted at order time.
/// </summary>
public class Order : AggregateRoot, IAuditable
{
    private readonly List<OrderItem> _items = new();

    public Guid CustomerId { get; private set; }
    public DateTime OrderDate { get; private set; }
    public OrderStatus Status { get; private set; }
    public DeliveryInfo? DeliveryInfo { get; private set; }
    public string? Notes { get; private set; }

    public IReadOnlyCollection<OrderItem> Items => _items;
    public decimal TotalAmount => _items.Sum(i => i.TotalPrice);

    private Order() { }

    public Order(Guid customerId, string? notes = null)
    {
        CustomerId = customerId;
        OrderDate = DateTime.UtcNow;
        Status = OrderStatus.Pending;
        Notes = notes;
    }

    /// <summary>
    /// Adds an item to the order by snapshotting product data at order time.
    /// If an item with the same ProductId and UnitPrice.Currency already exists, combines their quantities.
    /// Otherwise, creates a new OrderItem.
    /// </summary>
    public void AddItem(
        Guid productId,
        string productName,
        Money unitPrice,
        int quantity,
        string? notes = null)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOrderStateException("add items to", Status.ToString());

        if (quantity <= 0)
            throw new InvalidQuantityException();

        // Check for duplicate: same ProductId AND same currency (price snapshot)
        var existing = _items.FirstOrDefault(
            i => i.ProductId == productId && i.UnitPrice.Currency == unitPrice.Currency);

        if (existing is not null)
        {
            // Combine quantities for the same product and currency
            existing.UpdateQuantity(existing.Quantity + quantity);
        }
        else
        {
            // Create new item with the snapshot
            _items.Add(new OrderItem(Id, productId, productName, unitPrice, quantity, notes));
        }
    }

    public void RemoveItem(Guid productId)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOrderStateException("remove items from", Status.ToString());

        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item is not null)
            _items.Remove(item);
    }

    public void SetDeliveryInfo(DeliveryInfo deliveryInfo)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOrderStateException("change delivery information for", Status.ToString());

        ArgumentNullException.ThrowIfNull(deliveryInfo);
        DeliveryInfo = deliveryInfo;
    }

    public void EnsureReadyForPayment()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOrderStateException("place", Status.ToString());

        if (_items.Count == 0)
            throw new EmptyOrderException();
    }

    public void Confirm()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOrderStateException("confirm", Status.ToString());

        if (_items.Count == 0)
            throw new EmptyOrderException();

        Status = OrderStatus.Confirmed;
        RaiseDomainEvent(new OrderPlacedEvent(Id, CustomerId, TotalAmount));
    }

    public void MarkAsProcessing()
    {
        if (Status != OrderStatus.Confirmed)
            throw new InvalidOrderStateException("process", Status.ToString());

        Status = OrderStatus.Processing;
    }

    public void Complete()
    {
        if (Status != OrderStatus.Processing)
            throw new InvalidOrderStateException("complete", Status.ToString());

        Status = OrderStatus.Delivered;
    }

    public void Cancel(string reason)
    {
        if (Status == OrderStatus.Delivered)
            throw new InvalidOrderStateException("cancel", Status.ToString());

        if (Status == OrderStatus.Cancelled)
            throw new InvalidOrderStateException("cancel", Status.ToString());

        Status = OrderStatus.Cancelled;
        RaiseDomainEvent(new OrderCancelledEvent(Id, reason));
    }
}
