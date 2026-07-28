using SweetFlowerShop.Domain.Common;
using SweetFlowerShop.Domain.Exceptions;
using SweetFlowerShop.Domain.ValueObjects;

namespace SweetFlowerShop.Domain.Entities;

/// <summary>
/// Aggregate Root - Product represents a sellable item (flower, bouquet, plant, gift set).
/// Dependent entities: ProductImage
/// References Category by ID only (separate aggregate).
/// </summary>
public class Product : AggregateRoot, ISoftDeletable, IAuditable
{
    private readonly List<ProductImage> _images = new();

    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public Money Price { get; private set; } = null!;
    public Guid CategoryId { get; private set; }
    public bool IsAvailable { get; private set; } = true;
    public DateTime CreatedAt { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAtUtc { get; private set; }

    public IReadOnlyCollection<ProductImage> Images => _images;

    private Product() { }

    public Product(string name, string description, Money price, Guid categoryId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new EmptyNameException(nameof(Product));

        if (price.Amount <= 0)
            throw new InvalidPriceException();

        Name = name;
        Description = description;
        Price = price;
        CategoryId = categoryId;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new EmptyNameException(nameof(Product));

        Name = name;
        Description = description;
    }

    public void ChangePrice(Money newPrice)
    {
        if (newPrice.Amount <= 0)
            throw new InvalidPriceException();

        Price = newPrice;
    }

    public void ChangeCategory(Guid categoryId) => CategoryId = categoryId;

    public void Activate() => IsAvailable = true;

    public void Deactivate() => IsAvailable = false;

    public void AddImage(string url, bool isPrimary)
    {
        if (isPrimary)
        {
            foreach (var img in _images)
                img.SetNonPrimary();
        }

        _images.Add(new ProductImage(Id, url, isPrimary));
    }

    public void RemoveImage(Guid imageId)
    {
        var image = _images.FirstOrDefault(i => i.Id == imageId);
        if (image is not null)
            _images.Remove(image);
    }

    public void MarkAsDeleted()
    {
        IsDeleted = true;
        DeletedAtUtc = DateTime.UtcNow;
        Deactivate(); // Soft-deleted products are automatically unavailable
    }
}
