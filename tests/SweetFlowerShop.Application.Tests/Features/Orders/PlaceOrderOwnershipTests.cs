using SweetFlowerShop.Application.Features.Orders.PlaceOrder;
using SweetFlowerShop.Application.Tests.TestDoubles;
using SweetFlowerShop.Domain.Entities;
using SweetFlowerShop.Domain.ValueObjects;

namespace SweetFlowerShop.Application.Tests.Features.Orders;

public sealed class PlaceOrderOwnershipTests
{
    [Fact]
    public async Task AuthenticatedCustomer_OwnsCreatedOrder()
    {
        var customerId = Guid.NewGuid();
        var product = CreateProduct();
        var orderRepository = new OrderRepositoryFake();
        var unitOfWork = new UnitOfWorkFake();
        var handler = new PlaceOrderCommandHandler(
            orderRepository,
            new ProductRepositoryFake(product),
            new StubCurrentUserService(customerId),
            unitOfWork);

        var result = await handler.Handle(CreateCommand(product.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var order = Assert.Single(orderRepository.Added);
        Assert.Equal(customerId, order.CustomerId);
        Assert.Equal(customerId, result.Value!.CustomerId);
        Assert.Equal(1, unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public void AuthenticatedCustomer_CannotSupplyAnotherCustomerIdentity()
    {
        var commandProperties = typeof(PlaceOrderCommand).GetProperties();

        Assert.DoesNotContain(
            commandProperties,
            property => property.Name.Equals("CustomerId", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task AnonymousOrGuestRequest_IsRejectedBeforeProductLookupOrPersistence()
    {
        var product = CreateProduct();
        var productRepository = new ProductRepositoryFake(product);
        var orderRepository = new OrderRepositoryFake();
        var unitOfWork = new UnitOfWorkFake();
        var handler = new PlaceOrderCommandHandler(
            orderRepository,
            productRepository,
            new StubCurrentUserService(null),
            unitOfWork);

        var result = await handler.Handle(CreateCommand(product.Id), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("An authenticated customer is required.", result.Error);
        Assert.Equal(0, productRepository.GetByIdCalls);
        Assert.Empty(orderRepository.Added);
        Assert.Equal(0, unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task CreatedOrder_UsesServerProductAndRequestDeliverySnapshots()
    {
        var customerId = Guid.NewGuid();
        var product = CreateProduct();
        var orderRepository = new OrderRepositoryFake();
        var handler = new PlaceOrderCommandHandler(
            orderRepository,
            new ProductRepositoryFake(product),
            new StubCurrentUserService(customerId),
            new UnitOfWorkFake());

        var result = await handler.Handle(CreateCommand(product.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var order = Assert.Single(orderRepository.Added);
        var item = Assert.Single(order.Items);
        Assert.Equal(product.Name, item.ProductName);
        Assert.Equal(product.Price, item.UnitPrice);
        Assert.Equal("Jane Doe", order.DeliveryInfo.RecipientName);
        Assert.Equal("12 Garden Street", order.DeliveryInfo.Street);
    }

    private static PlaceOrderCommand CreateCommand(Guid productId) =>
        new(
            new DeliveryInfoRequest(
                "Jane Doe",
                "+98-912-123-4567",
                "12 Garden Street",
                "Tehran",
                "1234567890",
                null,
                "Happy birthday"),
            "Call before delivery",
            [new OrderItemRequest(productId, 2, "Red roses")]);

    private static Product CreateProduct() =>
        new(
            "Rose Bouquet",
            "Fresh roses",
            new Money(25m, "USD"),
            Guid.NewGuid());
}
