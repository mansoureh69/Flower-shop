using SweetFlowerShop.Domain.Common;
using SweetFlowerShop.Domain.Exceptions;

namespace SweetFlowerShop.Domain.Entities;

/// <summary>
/// Dependent entity of Order aggregate. Cannot be modified outside the aggregate.
/// </summary>
public class OrderItem : Entity
{
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    public decimal TotalPrice => UnitPrice * Quantity;

    private OrderItem() { }

    internal OrderItem(Guid orderId, Guid productId, string productName, decimal unitPrice, int quantity)
    {
        if (quantity <= 0)
            throw new InvalidQuantityException();

        OrderId = orderId;
        ProductId = productId;
        ProductName = productName;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }

    internal void UpdateQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new InvalidQuantityException();

        Quantity = quantity;
    }
}
