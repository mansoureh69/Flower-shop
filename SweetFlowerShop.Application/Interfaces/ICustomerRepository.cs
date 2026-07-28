using SweetFlowerShop.Domain.Entities;

namespace SweetFlowerShop.Application.Interfaces;

public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
}
