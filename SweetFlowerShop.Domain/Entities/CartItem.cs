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
    public string ProductName { get; private set; } = string.Empty;
    public int Quantity { get; private set; } = 0;
    public Money SnapshotPrice { get; private set; } = null!;

    private CartItem() { }

    internal CartItem(Guid cartId, Guid productId, string productName, Money price, int quantity)
    {
        if (quantity <= 0)
            throw new Exceptions.InvalidQuantityException();

        CartId = cartId;
        if (productId == Guid.Empty)
            throw new ArgumentException("Product ID is required.", nameof(productId));
        if (string.IsNullOrWhiteSpace(productName))
            throw new Exceptions.EmptyNameException(nameof(ProductName));
        ArgumentNullException.ThrowIfNull(price);

        ProductId = productId;
        ProductName = productName;
        Quantity = quantity;
        SnapshotPrice = price;
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
