using SweetFlowerShop.Application.Features.Carts.AddToCart;
using SweetFlowerShop.Application.Tests.TestDoubles;
using SweetFlowerShop.Domain.Entities;
using SweetFlowerShop.Domain.ValueObjects;

namespace SweetFlowerShop.Application.Tests.Features.Carts;

public sealed class AddToCartOwnershipTests
{
    [Fact]
    public async Task AuthenticatedCustomer_OwnsNewCart()
    {
        var customerId = Guid.NewGuid();
        var product = CreateProduct();
        var cartRepository = new CartRepositoryFake();
        var unitOfWork = new UnitOfWorkFake();
        var handler = new AddToCartCommandHandler(
            cartRepository,
            new ProductRepositoryFake(product),
            new StubCurrentUserService(customerId),
            unitOfWork);

        var result = await handler.Handle(new AddToCartCommand(product.Id, 2), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(customerId, cartRepository.RequestedCustomerId);
        var cart = Assert.Single(cartRepository.Added);
        Assert.Equal(customerId, cart.CustomerId);
        Assert.Equal(customerId, result.Value!.CustomerId);
        Assert.Equal(1, unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task AuthenticatedCustomer_UsesOnlyTheirExistingCart()
    {
        var customerId = Guid.NewGuid();
        var product = CreateProduct();
        var existingCart = new Cart(customerId);
        var cartRepository = new CartRepositoryFake(existingCart);
        var handler = new AddToCartCommandHandler(
            cartRepository,
            new ProductRepositoryFake(product),
            new StubCurrentUserService(customerId),
            new UnitOfWorkFake());

        var result = await handler.Handle(new AddToCartCommand(product.Id, 1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(cartRepository.Added);
        Assert.Same(existingCart, Assert.Single(cartRepository.Updated));
        Assert.Equal(customerId, cartRepository.RequestedCustomerId);
    }

    [Fact]
    public async Task AnonymousOrGuestRequest_IsRejectedBeforeProductOrCartAccess()
    {
        var product = CreateProduct();
        var productRepository = new ProductRepositoryFake(product);
        var cartRepository = new CartRepositoryFake();
        var unitOfWork = new UnitOfWorkFake();
        var handler = new AddToCartCommandHandler(
            cartRepository,
            productRepository,
            new StubCurrentUserService(null),
            unitOfWork);

        var result = await handler.Handle(new AddToCartCommand(product.Id, 1), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("An authenticated customer is required.", result.Error);
        Assert.Equal(0, productRepository.GetByIdCalls);
        Assert.Null(cartRepository.RequestedCustomerId);
        Assert.Empty(cartRepository.Added);
        Assert.Equal(0, unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public void CommandContract_DoesNotAcceptCustomerIdentity()
    {
        Assert.DoesNotContain(
            typeof(AddToCartCommand).GetProperties(),
            property => property.Name.Equals("CustomerId", StringComparison.OrdinalIgnoreCase));
    }

    private static Product CreateProduct() =>
        new(
            "Rose Bouquet",
            "Fresh roses",
            new Money(25m, "USD"),
            Guid.NewGuid());
}
