using SweetFlowerShop.Domain.Entities;

namespace SweetFlowerShop.Application.Interfaces;

public interface IPaymentRepository : IRepository<Payment>
{
    Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
}
