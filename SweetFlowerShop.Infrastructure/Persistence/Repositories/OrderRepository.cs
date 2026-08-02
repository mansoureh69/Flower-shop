using Microsoft.EntityFrameworkCore;
using SweetFlowerShop.Application.Interfaces;
using SweetFlowerShop.Domain.Entities;

namespace SweetFlowerShop.Infrastructure.Persistence.Repositories;

public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(FlowerShopDbContext context) : base(context) { }

    public override async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await DbSet.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Order>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
        => await DbSet.Where(o => o.CustomerId == customerId).Include(o => o.Items).ToListAsync(cancellationToken);
}
