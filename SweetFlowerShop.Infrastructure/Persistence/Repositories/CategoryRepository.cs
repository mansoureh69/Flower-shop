using Microsoft.EntityFrameworkCore;
using SweetFlowerShop.Application.Interfaces;
using SweetFlowerShop.Domain.Entities;

namespace SweetFlowerShop.Infrastructure.Persistence.Repositories;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(FlowerShopDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Category>> GetByParentIdAsync(Guid? parentId, CancellationToken cancellationToken = default)
        => await DbSet.Where(c => c.ParentCategoryId == parentId).ToListAsync(cancellationToken);
}
