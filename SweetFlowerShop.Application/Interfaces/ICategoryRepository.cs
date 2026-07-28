using SweetFlowerShop.Domain.Entities;

namespace SweetFlowerShop.Application.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    Task<IReadOnlyList<Category>> GetByParentIdAsync(Guid? parentId, CancellationToken cancellationToken = default);
}
