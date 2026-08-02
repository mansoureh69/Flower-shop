using SweetFlowerShop.Domain.Entities;
using SweetFlowerShop.Domain.Enums;
using SweetFlowerShop.Domain.Events;
using SweetFlowerShop.Domain.Exceptions;
using SweetFlowerShop.Domain.ValueObjects;

namespace SweetFlowerShop.Domain.Tests.Entities;

public sealed class OrderTests
{
    private const string Currency = "USD";

    [Fact]
    public void NewOrder_StartsPendingAndHasNoItems()
    {
        var customerId = Guid.NewGuid();

        var order = new Order(customerId, "Leave at reception");

        Assert.Equal(customerId, order.CustomerId);
        Assert.Equal(OrderStatus.Pending, order.Status);
        Assert.Empty(order.Items);
        Assert.Equal(0m, order.TotalAmount);
        Assert.Equal("Leave at reception", order.Notes);
    }

    [Fact]
    public void EnsureReadyForPayment_ThrowsWhenOrderHasNoItems()
    {
        var order = CreateOrder();

        Assert.Throws<EmptyOrderException>(() => order.EnsureReadyForPayment());
    }

    [Fact]
    public void AddItem_CapturesHistoricalProductDataAndCalculatesTotal()
    {
        var order = CreateOrder();
        var productId = Guid.NewGuid();

        order.AddItem(productId, "Rose Bouquet", new Money(25.50m, Currency), 2, "Red roses");

        var item = Assert.Single(order.Items);
        Assert.Equal(productId, item.ProductId);
        Assert.Equal("Rose Bouquet", item.ProductName);
        Assert.Equal(new Money(25.50m, Currency), item.UnitPrice);
        Assert.Equal(2, item.Quantity);
        Assert.Equal("Red roses", item.Notes);
        Assert.Equal(51m, item.TotalPrice);
        Assert.Equal(51m, order.TotalAmount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void AddItem_RejectsNonPositiveQuantity(int quantity)
    {
        var order = CreateOrder();

        Assert.Throws<InvalidQuantityException>(() =>
            order.AddItem(Guid.NewGuid(), "Rose Bouquet", new Money(10m, Currency), quantity));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void AddItem_RejectsMissingProductName(string productName)
    {
        var order = CreateOrder();

        Assert.Throws<EmptyNameException>(() =>
            order.AddItem(Guid.NewGuid(), productName, new Money(10m, Currency), 1));
    }

    [Fact]
    public void AddItem_CombinesSameProductAndCurrency()
    {
        var order = CreateOrder();
        var productId = Guid.NewGuid();

        order.AddItem(productId, "Rose Bouquet", new Money(10m, Currency), 1);
        order.AddItem(productId, "Rose Bouquet", new Money(10m, Currency), 2);

        var item = Assert.Single(order.Items);
        Assert.Equal(3, item.Quantity);
        Assert.Equal(30m, order.TotalAmount);
    }

    [Fact]
    public void AddItem_KeepsDifferentCurrenciesAsSeparateSnapshots()
    {
        var order = CreateOrder();
        var productId = Guid.NewGuid();

        order.AddItem(productId, "Rose Bouquet", new Money(10m, "USD"), 1);
        order.AddItem(productId, "Rose Bouquet", new Money(10m, "EUR"), 1);

        Assert.Equal(2, order.Items.Count);
    }

    [Fact]
    public void ExistingOrderItem_DoesNotChangeWhenProductChangesLater()
    {
        var categoryId = Guid.NewGuid();
        var product = new Product("Rose Bouquet", "Original", new Money(20m, Currency), categoryId);
        var order = CreateOrder();
        order.AddItem(product.Id, product.Name, product.Price, 1);

        product.UpdateDetails("Renamed Bouquet", "Updated");
        product.ChangePrice(new Money(40m, Currency));

        var item = Assert.Single(order.Items);
        Assert.Equal("Rose Bouquet", item.ProductName);
        Assert.Equal(new Money(20m, Currency), item.UnitPrice);
    }

    [Fact]
    public void Confirm_ConfirmsNonEmptyOrderAndRaisesOrderPlacedEvent()
    {
        var order = CreateOrderWithItem();

        order.Confirm();

        Assert.Equal(OrderStatus.Confirmed, order.Status);
        var domainEvent = Assert.IsType<OrderPlacedEvent>(Assert.Single(order.DomainEvents));
        Assert.Equal(order.Id, domainEvent.OrderId);
        Assert.Equal(order.CustomerId, domainEvent.CustomerId);
        Assert.Equal(order.TotalAmount, domainEvent.TotalAmount);
    }

    [Fact]
    public void Confirm_RejectsRepeatedConfirmation()
    {
        var order = CreateOrderWithItem();
        order.Confirm();

        Assert.Throws<InvalidOrderStateException>(() => order.Confirm());
    }

    [Fact]
    public void ConfirmedOrder_RejectsItemAndDeliveryChanges()
    {
        var order = CreateOrderWithItem();
        order.Confirm();

        Assert.Throws<InvalidOrderStateException>(() =>
            order.AddItem(Guid.NewGuid(), "Tulips", new Money(15m, Currency), 1));
        Assert.Throws<InvalidOrderStateException>(() => order.RemoveItem(order.Items.Single().ProductId));
        Assert.Throws<InvalidOrderStateException>(() => order.SetDeliveryInfo(CreateDeliveryInfo()));
    }

    [Fact]
    public void ProcessingOrder_CanBeCompleted()
    {
        var order = CreateOrderWithItem();
        order.Confirm();
        order.MarkAsProcessing();

        order.Complete();

        Assert.Equal(OrderStatus.Delivered, order.Status);
    }

    [Fact]
    public void Cancel_RaisesEventAndMakesCancellationTerminal()
    {
        var order = CreateOrderWithItem();

        order.Cancel("Customer request");

        Assert.Equal(OrderStatus.Cancelled, order.Status);
        var domainEvent = Assert.IsType<OrderCancelledEvent>(Assert.Single(order.DomainEvents));
        Assert.Equal("Customer request", domainEvent.Reason);
        Assert.Throws<InvalidOrderStateException>(() => order.Cancel("Again"));
    }

    [Fact]
    public void DeliveredOrder_CannotBeCancelled()
    {
        var order = CreateOrderWithItem();
        order.Confirm();
        order.MarkAsProcessing();
        order.Complete();

        Assert.Throws<InvalidOrderStateException>(() => order.Cancel("Too late"));
    }

    private static Order CreateOrder() => new(Guid.NewGuid());

    private static Order CreateOrderWithItem()
    {
        var order = CreateOrder();
        order.AddItem(Guid.NewGuid(), "Rose Bouquet", new Money(25m, Currency), 2);
        return order;
    }

    private static DeliveryInfo CreateDeliveryInfo() =>
        new("Jane Doe", "+1-555-0100", "12 Garden Street", "Tehran", "12345");
}
