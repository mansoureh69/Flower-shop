using SweetFlowerShop.Domain.Common;
using SweetFlowerShop.Domain.Exceptions;

namespace SweetFlowerShop.Domain.Entities;

/// <summary>
/// Aggregate Root - Category groups products (e.g., Roses, Bouquets, Indoor Plants).
/// Supports up to 3 levels of hierarchy. Does NOT hold Products — that's a query concern.
/// Deletion guard (has children/products) is enforced by CategoryDeletionService.
/// </summary>
public class Category : AggregateRoot, ISoftDeletable, IAuditable
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public Guid? ParentCategoryId { get; private set; }
    public int Level { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAtUtc { get; private set; }

    private Category() { }

    public Category(string name, string description, Guid? parentCategoryId = null, int level = 1)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new EmptyNameException(nameof(Category));

        if (level < 1 || level > InvalidCategoryLevelException.MaxLevel)
            throw new InvalidCategoryLevelException();

        Name = name;
        Description = description;
        ParentCategoryId = parentCategoryId;
        Level = level;
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new EmptyNameException(nameof(Category));

        Name = name;
    }

    public void UpdateDescription(string description) => Description = description;

    public void MarkAsDeleted()
    {
        IsDeleted = true;
        DeletedAtUtc = DateTime.UtcNow;
    }
}
