using SweetFlowerShop.Domain.Entities;

namespace SweetFlowerShop.Application.Interfaces;

public interface IProductRepository : IRepository<Product>
{
    Task<IReadOnlyList<Product>> GetByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetAvailableAsync(CancellationToken cancellationToken = default);
    Task<bool> AnyByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);
}
