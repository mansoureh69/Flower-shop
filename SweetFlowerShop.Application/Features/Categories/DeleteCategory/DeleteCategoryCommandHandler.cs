using MediatR;
using SweetFlowerShop.Application.Common;
using SweetFlowerShop.Application.Interfaces;

namespace SweetFlowerShop.Application.Features.Categories.DeleteCategory;

public sealed class DeleteCategoryCommandHandler(
    ICategoryRepository categories,
    IProductRepository products,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteCategoryCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await categories.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category is null)
            return Result<bool>.Failure("Category not found.");

        var children = await categories.GetByParentIdAsync(category.Id, cancellationToken);
        var hasProducts = await products.AnyByCategoryAsync(category.Id, cancellationToken);
        category.MarkAsDeleted(children.Count != 0, hasProducts);
        categories.Update(category);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }
}
