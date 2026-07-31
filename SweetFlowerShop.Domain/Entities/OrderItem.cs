using SweetFlowerShop.Domain.Common;
using SweetFlowerShop.Domain.Exceptions;
using SweetFlowerShop.Domain.ValueObjects;

namespace SweetFlowerShop.Domain.Entities;

/// <summary>
/// Dependent entity of Order aggregate. Cannot be modified outside the aggregate.
/// </summary>
public class OrderItem : Entity
{
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public Product? Product { get; private set; }
    public Money UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    public string Notes { get; private set; } = string.Empty;
    public decimal TotalPrice => UnitPrice.Amount * Quantity;

    private OrderItem() { }

   

    internal OrderItem(Guid orderId, Product product, int quantity,string? notes)
    {
        if (quantity <= 0)
            throw new InvalidQuantityException();

        if (product is null )
            throw new EmptyNameException("ProductName");

       

        OrderId = orderId;
        ProductId = product.Id;
        Product = product;
        Notes = notes ?? string.Empty;
        UnitPrice = product.Price;
        Quantity = quantity;
    }


    internal void UpdateQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new InvalidQuantityException();

        Quantity = quantity;
    }
}
