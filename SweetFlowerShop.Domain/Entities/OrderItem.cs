using SweetFlowerShop.Domain.Common;
using SweetFlowerShop.Domain.Exceptions;
using SweetFlowerShop.Domain.ValueObjects;

namespace SweetFlowerShop.Domain.Entities;

/// <summary>
/// Dependent entity of Order aggregate. Cannot be modified outside the aggregate.
/// Snapshots product data at order time to ensure historical immutability.
/// Does NOT have a navigation property to Product; ProductId is a shadow reference only.
/// </summary>
public class OrderItem : Entity
{
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public Money UnitPrice { get; private set; } = null!;
    public int Quantity { get; private set; }
    public string? Notes { get; private set; }

    public decimal TotalPrice => UnitPrice.Amount * Quantity;

    private OrderItem() { }

    /// <summary>
    /// Constructs an OrderItem by snapshotting product data.
    /// Called internally by Order aggregate only.
    /// </summary>
    internal OrderItem(
        Guid orderId,
        Guid productId,
        string productName,
        Money unitPrice,
        int quantity,
        string? notes = null)
    {
        // Validate all arguments
        if (quantity <= 0)
            throw new InvalidQuantityException();

        if (string.IsNullOrWhiteSpace(productName))
            throw new EmptyNameException(nameof(ProductName));

        if (unitPrice is null)
            throw new ArgumentNullException(nameof(unitPrice));

        if (unitPrice.Amount < 0)
            throw new InvalidPriceException();

        OrderId = orderId;
        ProductId = productId;
        ProductName = productName;
        UnitPrice = unitPrice;
        Quantity = quantity;
        Notes = notes;
    }

    /// <summary>
    /// Updates quantity for the same product and price combination.
    /// Only called when combining duplicate items.
    /// </summary>
    internal void UpdateQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new InvalidQuantityException();

        Quantity = quantity;
    }
}
