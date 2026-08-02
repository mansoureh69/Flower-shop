using MediatR;
using SweetFlowerShop.Application.Common;

namespace SweetFlowerShop.Application.Features.Categories.DeleteCategory;

public sealed record DeleteCategoryCommand(Guid CategoryId) : IRequest<Result<bool>>;
