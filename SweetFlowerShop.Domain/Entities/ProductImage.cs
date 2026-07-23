using SweetFlowerShop.Domain.Common;

namespace SweetFlowerShop.Domain.Entities;

/// <summary>
/// Dependent entity of Product aggregate.
/// </summary>
public class ProductImage : Entity
{
    public Guid ProductId { get; private set; }
    public string Url { get; private set; } = string.Empty;
    public bool IsPrimary { get; private set; }

    private ProductImage() { }

    internal ProductImage(Guid productId, string url, bool isPrimary)
    {
        ProductId = productId;
        Url = url;
        IsPrimary = isPrimary;
    }

    internal void SetNonPrimary() => IsPrimary = false;
}
