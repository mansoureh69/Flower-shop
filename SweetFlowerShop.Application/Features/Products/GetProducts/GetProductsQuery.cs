using MediatR;
using SweetFlowerShop.Application.Common;
using SweetFlowerShop.Application.Features.Products.Common;

namespace SweetFlowerShop.Application.Features.Products.GetProducts;

public record GetProductsQuery(Guid? CategoryId = null, bool? AvailableOnly = null)
    : IRequest<Result<IReadOnlyList<ProductResponse>>>;
