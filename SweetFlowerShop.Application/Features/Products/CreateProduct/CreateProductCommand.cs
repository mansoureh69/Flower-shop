using MediatR;
using SweetFlowerShop.Application.Common;
using SweetFlowerShop.Application.Features.Products.Common;

namespace SweetFlowerShop.Application.Features.Products.CreateProduct;

public record CreateProductCommand(
    string Name,
    string Description,
    decimal Price,
    string Currency,
    Guid CategoryId,
    bool IsAvailable = true) : IRequest<Result<ProductResponse>>;
