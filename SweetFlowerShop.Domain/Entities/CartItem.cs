using SweetFlowerShop.Domain.Common;

namespace SweetFlowerShop.Domain.Entities;

/// <summary>
/// Dependent entity of Cart aggregate. Holds ProductId and Quantity only.
/// Price is NOT stored — always reflects current product price (resolved at checkout).
/// </summary>
public class CartItem : Entity
{
    public Guid CartId { get; private set; }
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public int Price     { get; private set; }

    private CartItem() { }

    internal CartItem(Guid cartId, Guid productId, int quantity)
    {
        if (quantity <= 0)
            throw new Exceptions.InvalidQuantityException();

        CartId = cartId;
        ProductId = productId;
        Quantity = quantity;
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
