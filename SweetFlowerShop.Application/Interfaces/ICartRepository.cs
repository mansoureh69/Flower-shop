using SweetFlowerShop.Domain.Entities;

namespace SweetFlowerShop.Application.Interfaces;

public interface ICartRepository : IRepository<Cart>
{
    Task<Cart?> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
}
