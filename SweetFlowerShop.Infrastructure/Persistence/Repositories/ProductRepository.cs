using Microsoft.EntityFrameworkCore;
using SweetFlowerShop.Application.Interfaces;
using SweetFlowerShop.Domain.Entities;

namespace SweetFlowerShop.Infrastructure.Persistence.Repositories;

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(FlowerShopDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Product>> GetByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
        => await DbSet.Where(p => p.CategoryId == categoryId).Include(p => p.Images).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Product>> GetAvailableAsync(CancellationToken cancellationToken = default)
        => await DbSet.Where(p => p.IsAvailable).Include(p => p.Images).ToListAsync(cancellationToken);
}
