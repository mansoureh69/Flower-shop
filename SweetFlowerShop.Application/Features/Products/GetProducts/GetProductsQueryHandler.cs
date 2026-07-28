using MediatR;
using SweetFlowerShop.Application.Common;
using SweetFlowerShop.Application.Features.Products.Common;
using SweetFlowerShop.Application.Features.Products.CreateProduct;
using SweetFlowerShop.Application.Interfaces;

namespace SweetFlowerShop.Application.Features.Products.GetProducts;

public sealed class GetProductsQueryHandler(IProductRepository productRepository)
    : IRequestHandler<GetProductsQuery, Result<IReadOnlyList<ProductResponse>>>
{
    public async Task<Result<IReadOnlyList<ProductResponse>>> Handle(
        GetProductsQuery request, CancellationToken cancellationToken)
    {
        var products = request switch
        {
            { CategoryId: not null } => await productRepository.GetByCategoryAsync(request.CategoryId.Value, cancellationToken),
            { AvailableOnly: true } => await productRepository.GetAvailableAsync(cancellationToken),
            _ => await productRepository.GetAllAsync(cancellationToken)
        };

        var response = products.Select(p => p.ToResponse()).ToList();
        return Result<IReadOnlyList<ProductResponse>>.Success(response);
    }
}
