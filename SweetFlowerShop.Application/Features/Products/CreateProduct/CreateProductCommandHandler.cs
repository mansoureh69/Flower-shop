using MediatR;
using SweetFlowerShop.Application.Common;
using SweetFlowerShop.Application.Features.Products.Common;
using SweetFlowerShop.Application.Interfaces;
using SweetFlowerShop.Domain.Entities;
using SweetFlowerShop.Domain.ValueObjects;

namespace SweetFlowerShop.Application.Features.Products.CreateProduct;

public sealed class CreateProductCommandHandler(
    IProductRepository productRepository,
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateProductCommand, Result<ProductResponse>>
{
    public async Task<Result<ProductResponse>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var category = await categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category is null)
            return Result<ProductResponse>.Failure("Category not found.");

        var product = new Product(
            request.Name,
            request.Description,
            new Money(request.Price, request.Currency),
            request.CategoryId);

        if (!request.IsAvailable)
            product.Deactivate();

        await productRepository.AddAsync(product, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ProductResponse>.Success(product.ToResponse());
    }
}

internal static class ProductMappingExtensions
{
    public static ProductResponse ToResponse(this Product product) => new(
        product.Id,
        product.Name,
        product.Description,
        product.Price.Amount,
        product.Price.Currency,
        product.CategoryId,
        product.IsAvailable,
        product.CreatedAt,
        product.Images.Select(i => new ProductImageResponse(i.Id, i.Url, i.IsPrimary)).ToList());
}
