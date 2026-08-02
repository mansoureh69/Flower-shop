using SweetFlowerShop.Domain.Entities;
using SweetFlowerShop.Domain.Enums;
using SweetFlowerShop.Domain.Events;
using SweetFlowerShop.Domain.Exceptions;
using SweetFlowerShop.Domain.ValueObjects;

namespace SweetFlowerShop.Domain.Tests.Entities;

public sealed class OrderTests
{
    [Fact]
    public void Place_CreatesCompletePendingPaymentOrderAndRaisesPlacedEvent()
    {
        var customerId = Guid.NewGuid();

        var order = Order.Place(
            customerId,
            CreateDeliveryInfo(),
            [CreateLine(price: 25.50m, quantity: 2)],
            "Leave at reception");

        Assert.Equal(customerId, order.CustomerId);
        Assert.Equal(OrderStatus.PendingPayment, order.Status);
        Assert.Equal(51m, order.TotalAmount);
        Assert.Equal("Leave at reception", order.Notes);
        Assert.Equal(CreateDeliveryInfo(), order.DeliveryInfo);

        var domainEvent = Assert.IsType<OrderPlacedEvent>(Assert.Single(order.DomainEvents));
        Assert.Equal(order.Id, domainEvent.OrderId);
        Assert.Equal(customerId, domainEvent.CustomerId);
        Assert.Equal(51m, domainEvent.TotalAmount);
    }

    [Fact]
    public void Place_RejectsEmptyOrder()
    {
        Assert.Throws<EmptyOrderException>(() =>
            Order.Place(Guid.NewGuid(), CreateDeliveryInfo(), []));
    }

    [Fact]
    public void Place_RejectsMissingCustomerIdentity()
    {
        Assert.Throws<ArgumentException>(() =>
            Order.Place(Guid.Empty, CreateDeliveryInfo(), [CreateLine()]));
    }

    [Fact]
    public void Place_RejectsMissingDeliverySnapshot()
    {
        Assert.Throws<ArgumentNullException>(() =>
            Order.Place(Guid.NewGuid(), null!, [CreateLine()]));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Place_RejectsNonPositiveQuantity(int quantity)
    {
        Assert.Throws<InvalidQuantityException>(() =>
            Order.Place(Guid.NewGuid(), CreateDeliveryInfo(), [CreateLine(quantity: quantity)]));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Place_RejectsMissingProductName(string productName)
    {
        Assert.Throws<EmptyNameException>(() =>
            Order.Place(Guid.NewGuid(), CreateDeliveryInfo(), [CreateLine(productName: productName)]));
    }

    [Fact]
    public void Place_CapturesHistoricalProductData()
    {
        var productId = Guid.NewGuid();

        var order = Order.Place(
            Guid.NewGuid(),
            CreateDeliveryInfo(),
            [new OrderLineSnapshot(productId, "Rose Bouquet", new Money(25.50m, "USD"), 2, "Red roses")]);

        var item = Assert.Single(order.Items);
        Assert.Equal(productId, item.ProductId);
        Assert.Equal("Rose Bouquet", item.ProductName);
        Assert.Equal(new Money(25.50m, "USD"), item.UnitPrice);
        Assert.Equal(2, item.Quantity);
        Assert.Equal("Red roses", item.Notes);
        Assert.Equal(51m, item.TotalPrice);
    }

    [Fact]
    public void Place_CombinesOnlyIdenticalSnapshots()
    {
        var productId = Guid.NewGuid();
        var snapshot = CreateLine(productId, price: 10m, quantity: 1);

        var order = Order.Place(
            Guid.NewGuid(),
            CreateDeliveryInfo(),
            [snapshot, snapshot with { Quantity = 2 }]);

        var item = Assert.Single(order.Items);
        Assert.Equal(3, item.Quantity);
        Assert.Equal(30m, order.TotalAmount);
    }

    [Fact]
    public void Place_KeepsDifferentPriceSnapshotsSeparate()
    {
        var productId = Guid.NewGuid();

        var order = Order.Place(
            Guid.NewGuid(),
            CreateDeliveryInfo(),
            [CreateLine(productId, price: 10m), CreateLine(productId, price: 12m)]);

        Assert.Equal(2, order.Items.Count);
    }

    [Fact]
    public void ExistingOrderItem_DoesNotChangeWhenProductChangesLater()
    {
        var product = new Product(
            "Rose Bouquet",
            "Original",
            new Money(20m, "USD"),
            Guid.NewGuid());
        var order = Order.Place(
            Guid.NewGuid(),
            CreateDeliveryInfo(),
            [new OrderLineSnapshot(product.Id, product.Name, product.Price, 1)]);

        product.UpdateDetails("Renamed Bouquet", "Updated");
        product.ChangePrice(new Money(40m, "USD"));

        var item = Assert.Single(order.Items);
        Assert.Equal("Rose Bouquet", item.ProductName);
        Assert.Equal(new Money(20m, "USD"), item.UnitPrice);
    }

    [Fact]
    public void ConfirmPayment_MovesPendingPaymentToConfirmedAndRaisesEvent()
    {
        var order = CreateOrder();
        order.ClearDomainEvents();

        order.ConfirmPayment();

        Assert.Equal(OrderStatus.Confirmed, order.Status);
        var domainEvent = Assert.IsType<OrderConfirmedEvent>(Assert.Single(order.DomainEvents));
        Assert.Equal(order.Id, domainEvent.OrderId);
        Assert.Equal(order.CustomerId, domainEvent.CustomerId);
    }

    [Fact]
    public void ConfirmPayment_RejectsRepeatedConfirmation()
    {
        var order = CreateConfirmedOrder();

        Assert.Throws<InvalidOrderStateException>(() => order.ConfirmPayment());
    }

    [Fact]
    public void Fulfilment_UsesEveryRequiredTransitionInOrder()
    {
        var order = CreateConfirmedOrder();

        order.StartPreparing();
        Assert.Equal(OrderStatus.Preparing, order.Status);

        order.MarkReadyForDelivery();
        Assert.Equal(OrderStatus.ReadyForDelivery, order.Status);

        order.MarkOutForDelivery();
        Assert.Equal(OrderStatus.OutForDelivery, order.Status);

        order.MarkDelivered();
        Assert.Equal(OrderStatus.Delivered, order.Status);
    }

    [Fact]
    public void StartPreparing_RequiresConfirmedOrder()
    {
        var order = CreateOrder();

        Assert.Throws<InvalidOrderStateException>(() => order.StartPreparing());
    }

    [Fact]
    public void MarkReadyForDelivery_RequiresPreparingOrder()
    {
        var order = CreateConfirmedOrder();

        Assert.Throws<InvalidOrderStateException>(() => order.MarkReadyForDelivery());
    }

    [Fact]
    public void MarkOutForDelivery_RequiresReadyOrder()
    {
        var order = CreateConfirmedOrder();
        order.StartPreparing();

        Assert.Throws<InvalidOrderStateException>(() => order.MarkOutForDelivery());
    }

    [Fact]
    public void MarkDelivered_RequiresOutForDeliveryOrder()
    {
        var order = CreateConfirmedOrder();
        order.StartPreparing();
        order.MarkReadyForDelivery();

        Assert.Throws<InvalidOrderStateException>(() => order.MarkDelivered());
    }

    [Fact]
    public void SetDeliveryInfo_IsAllowedOnlyBeforePaymentConfirmation()
    {
        var order = CreateOrder();
        var deliveryInfo = CreateDeliveryInfo();

        order.SetDeliveryInfo(deliveryInfo);
        Assert.Equal(deliveryInfo, order.DeliveryInfo);

        order.ConfirmPayment();
        Assert.Throws<InvalidOrderStateException>(() => order.SetDeliveryInfo(CreateDeliveryInfo()));
    }

    [Fact]
    public void CancelUnpaid_CancelsPendingPaymentOrderAndRaisesEvent()
    {
        var order = CreateOrder();
        order.ClearDomainEvents();

        order.CancelUnpaid("Customer request");

        Assert.Equal(OrderStatus.Cancelled, order.Status);
        var domainEvent = Assert.IsType<OrderCancelledEvent>(Assert.Single(order.DomainEvents));
        Assert.Equal("Customer request", domainEvent.Reason);
    }

    [Fact]
    public void CancelUnpaid_RejectsPaidOrder()
    {
        var order = CreateConfirmedOrder();

        Assert.Throws<InvalidOrderStateException>(() => order.CancelUnpaid("Customer request"));
    }

    [Fact]
    public void CancelAfterRefund_RejectsUnpaidOrder()
    {
        var order = CreateOrder();

        Assert.Throws<InvalidOrderStateException>(() => order.CancelAfterRefund("Customer request"));
    }

    [Fact]
    public void CancelAfterRefund_AllowsPaidNonTerminalStates()
    {
        var confirmed = CreateConfirmedOrder();
        confirmed.CancelAfterRefund("Refund complete");

        var preparing = CreateConfirmedOrder();
        preparing.StartPreparing();
        preparing.CancelAfterRefund("Refund complete");

        var ready = CreateConfirmedOrder();
        ready.StartPreparing();
        ready.MarkReadyForDelivery();
        ready.CancelAfterRefund("Refund complete");

        var outForDelivery = CreateConfirmedOrder();
        outForDelivery.StartPreparing();
        outForDelivery.MarkReadyForDelivery();
        outForDelivery.MarkOutForDelivery();
        outForDelivery.CancelAfterRefund("Refund complete");

        Assert.All(
            new[] { confirmed, preparing, ready, outForDelivery },
            order => Assert.Equal(OrderStatus.Cancelled, order.Status));
    }

    [Fact]
    public void Cancellation_RequiresReason()
    {
        var order = CreateOrder();

        Assert.Throws<ArgumentException>(() => order.CancelUnpaid("   "));
        Assert.Equal(OrderStatus.PendingPayment, order.Status);
    }

    [Fact]
    public void DeliveredAndCancelledOrdersAreTerminal()
    {
        var delivered = CreateDeliveredOrder();
        Assert.Throws<InvalidOrderStateException>(() => delivered.MarkDelivered());
        Assert.Throws<InvalidOrderStateException>(() => delivered.CancelAfterRefund("Too late"));
        Assert.Throws<InvalidOrderStateException>(() => delivered.SetDeliveryInfo(CreateDeliveryInfo()));

        var cancelled = CreateOrder();
        cancelled.CancelUnpaid("Customer request");
        Assert.Throws<InvalidOrderStateException>(() => cancelled.ConfirmPayment());
        Assert.Throws<InvalidOrderStateException>(() => cancelled.StartPreparing());
        Assert.Throws<InvalidOrderStateException>(() => cancelled.CancelUnpaid("Again"));
        Assert.Throws<InvalidOrderStateException>(() => cancelled.SetDeliveryInfo(CreateDeliveryInfo()));
    }

    private static OrderLineSnapshot CreateLine(
        Guid? productId = null,
        string productName = "Rose Bouquet",
        decimal price = 25m,
        int quantity = 1) =>
        new(productId ?? Guid.NewGuid(), productName, new Money(price, "USD"), quantity);

    private static Order CreateOrder() =>
        Order.Place(Guid.NewGuid(), CreateDeliveryInfo(), [CreateLine()]);

    private static Order CreateConfirmedOrder()
    {
        var order = CreateOrder();
        order.ConfirmPayment();
        return order;
    }

    private static Order CreateDeliveredOrder()
    {
        var order = CreateConfirmedOrder();
        order.StartPreparing();
        order.MarkReadyForDelivery();
        order.MarkOutForDelivery();
        order.MarkDelivered();
        return order;
    }

    private static DeliveryInfo CreateDeliveryInfo() =>
        new("Jane Doe", "+1-555-0100", "12 Garden Street", "Tehran", "12345");
}
