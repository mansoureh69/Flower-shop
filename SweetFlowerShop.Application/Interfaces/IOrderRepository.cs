using SweetFlowerShop.Domain.Entities;

namespace SweetFlowerShop.Application.Interfaces;

public interface IOrderRepository : IRepository<Order>
{
    Task<IReadOnlyList<Order>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
}
