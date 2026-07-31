using SweetFlowerShop.Domain.Common;
using SweetFlowerShop.Domain.ValueObjects;

namespace SweetFlowerShop.Domain.Entities;

/// <summary>
/// Dependent entity of Cart aggregate. Holds ProductId, Quantity, and snapshotted Price.
/// Price is captured at add time to ensure consistency during checkout.
/// </summary>
public class CartItem : Entity
{
    public Guid CartId { get; private set; }
    public Guid ProductId { get; private set; }
    public Product? Product { get; private set; }
    public int Quantity { get; private set; } = 0;
    public Money SnapshotPrice { get; private set; } = null!;

    private CartItem() { }

    internal CartItem(Guid cartId, Product product, int quantity)
    {
        if (quantity <= 0)
            throw new Exceptions.InvalidQuantityException();

        CartId = cartId;
        ProductId = product.Id;
        Product = product;
        Quantity = quantity;
        SnapshotPrice = product.Price;
    }

    internal void IncreaseQuantity(int amount)
    {
        if (amount <= 0)
            throw new Exceptions.InvalidQuantityException();

        Quantity += amount;
    }

    internal void ChangeQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new Exceptions.InvalidQuantityException();

        Quantity = quantity;
    }
}
