using Microsoft.EntityFrameworkCore;
using SweetFlowerShop.Application.Interfaces;
using SweetFlowerShop.Domain.Entities;

namespace SweetFlowerShop.Infrastructure.Persistence.Repositories;

public class PaymentRepository : Repository<Payment>, IPaymentRepository
{
    public PaymentRepository(FlowerShopDbContext context) : base(context) { }

    public async Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
        => await DbSet.Where(p => p.OrderId == orderId).Include(p => p.Transactions).FirstOrDefaultAsync(cancellationToken);
}
