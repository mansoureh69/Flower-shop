using SweetFlowerShop.Domain.Common;
using SweetFlowerShop.Domain.Exceptions;

namespace SweetFlowerShop.Domain.Entities;

/// <summary>
/// Aggregate Root - Represents a customer's shopping cart.
/// Dependent entities: CartItem
/// </summary>
public class Cart : AggregateRoot
{
    private readonly List<CartItem> _items = new();

    public Guid CustomerId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyCollection<CartItem> Items => _items;

    private Cart() { }

    public Cart(Guid customerId)
    {
        CustomerId = customerId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddItem(Guid productId, Product product, int quantity)
    {
        if (quantity <= 0)
            throw new InvalidQuantityException();

        var existing = _items.FirstOrDefault(i => i.ProductId == productId);
        if (existing is not null)
        {
            existing.IncreaseQuantity(quantity);
        }
        else
        {
            _items.Add(new CartItem(Id, product, quantity));
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateItemQuantity(Guid productId, int quantity)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == productId)
            ?? throw new InvalidOperationException("Item not found in cart.");

        item.ChangeQuantity(quantity);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveItem(Guid productId)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item is not null)
        {
            _items.Remove(item);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void Clear()
    {
        _items.Clear();
        UpdatedAt = DateTime.UtcNow;
    }
}
