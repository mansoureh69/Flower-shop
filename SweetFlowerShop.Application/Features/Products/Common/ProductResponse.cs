namespace SweetFlowerShop.Application.Features.Products.Common;

public record ProductResponse(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    string Currency,
    Guid CategoryId,
    bool IsAvailable,
    DateTime CreatedAt,
    IReadOnlyList<ProductImageResponse> Images);

public record ProductImageResponse(
    Guid Id,
    string Url,
    bool IsPrimary);
