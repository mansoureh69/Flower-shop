using SweetFlowerShop.Domain.Common;

namespace SweetFlowerShop.Domain.Events;

public sealed record OrderConfirmedEvent(Guid OrderId, Guid CustomerId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
