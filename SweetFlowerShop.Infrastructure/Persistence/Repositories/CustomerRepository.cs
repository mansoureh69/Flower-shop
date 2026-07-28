using Microsoft.EntityFrameworkCore;
using SweetFlowerShop.Application.Interfaces;
using SweetFlowerShop.Domain.Entities;

namespace SweetFlowerShop.Infrastructure.Persistence.Repositories;

public class CustomerRepository : Repository<Customer>, ICustomerRepository
{
    public CustomerRepository(FlowerShopDbContext context) : base(context) { }

    public async Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => await DbSet.FirstOrDefaultAsync(c => c.Email.Value == email, cancellationToken);
}
