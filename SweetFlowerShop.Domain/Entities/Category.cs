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

    public Category(string name, string description)
        : this(name, description, null, 1)
    {
    }

    private Category(string name, string description, Guid? parentCategoryId, int level)
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

    public static Category CreateChild(string name, string description, Category parent)
    {
        ArgumentNullException.ThrowIfNull(parent);
        if (parent.IsDeleted)
            throw new InvalidOperationException("Cannot add a child to a deleted category.");
        if (parent.Level >= InvalidCategoryLevelException.MaxLevel)
            throw new InvalidCategoryLevelException();

        return new Category(name, description, parent.Id, parent.Level + 1);
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new EmptyNameException(nameof(Category));

        Name = name;
    }

    public void UpdateDescription(string description) => Description = description;

    public void MarkAsDeleted(bool hasChildren, bool hasProducts)
    {
        if (hasChildren)
            throw new InvalidOperationException("A category with child categories cannot be deleted.");
        if (hasProducts)
            throw new InvalidOperationException("A category containing products cannot be deleted.");
        IsDeleted = true;
        DeletedAtUtc = DateTime.UtcNow;
    }

    void ISoftDeletable.MarkAsDeleted() =>
        throw new InvalidOperationException("Category deletion requires child and product checks.");
}
