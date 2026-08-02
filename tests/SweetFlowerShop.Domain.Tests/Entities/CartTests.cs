using SweetFlowerShop.Domain.Entities;
using SweetFlowerShop.Domain.Exceptions;
using SweetFlowerShop.Domain.ValueObjects;

namespace SweetFlowerShop.Domain.Tests.Entities;

public sealed class CartTests
{
    [Fact]
    public void AddItem_CapturesPriceAtTheTimeItIsAdded()
    {
        var product = CreateProduct(20m);
        var cart = new Cart(Guid.NewGuid());

        AddProduct(cart, product, 2);
        product.ChangePrice(new Money(30m, "USD"));

        var item = Assert.Single(cart.Items);
        Assert.Equal(new Money(20m, "USD"), item.SnapshotPrice);
        Assert.Equal(2, item.Quantity);
    }

    [Fact]
    public void AddItem_IncreasesQuantityWhenProductAlreadyExists()
    {
        var product = CreateProduct();
        var cart = new Cart(Guid.NewGuid());

        AddProduct(cart, product, 1);
        AddProduct(cart, product, 2);

        var item = Assert.Single(cart.Items);
        Assert.Equal(3, item.Quantity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AddItem_RejectsNonPositiveQuantity(int quantity)
    {
        var product = CreateProduct();
        var cart = new Cart(Guid.NewGuid());

        Assert.Throws<InvalidQuantityException>(() => AddProduct(cart, product, quantity));
    }

    [Fact]
    public void UpdateItemQuantity_ChangesExistingItem()
    {
        var product = CreateProduct();
        var cart = new Cart(Guid.NewGuid());
        AddProduct(cart, product, 1);

        cart.UpdateItemQuantity(product.Id, 4);

        Assert.Equal(4, Assert.Single(cart.Items).Quantity);
    }

    [Fact]
    public void UpdateItemQuantity_RejectsUnknownProduct()
    {
        var cart = new Cart(Guid.NewGuid());

        Assert.Throws<InvalidOperationException>(() => cart.UpdateItemQuantity(Guid.NewGuid(), 2));
    }

    [Fact]
    public void RemoveAndClear_ModifyOnlyTheCartCollection()
    {
        var first = CreateProduct();
        var second = CreateProduct();
        var cart = new Cart(Guid.NewGuid());
        AddProduct(cart, first, 1);
        AddProduct(cart, second, 1);

        cart.RemoveItem(first.Id);
        Assert.Equal(second.Id, Assert.Single(cart.Items).ProductId);

        cart.Clear();
        Assert.Empty(cart.Items);
    }

    private static Product CreateProduct(decimal price = 20m) =>
        new("Rose Bouquet", "Fresh roses", new Money(price, "USD"), Guid.NewGuid());

    private static void AddProduct(Cart cart, Product product, int quantity) =>
        cart.AddItem(product.Id, product.Name, product.Price, quantity);
}
