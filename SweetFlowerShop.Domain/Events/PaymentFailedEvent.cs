using SweetFlowerShop.Domain.Common;

namespace SweetFlowerShop.Domain.Events;

public record PaymentFailedEvent(Guid PaymentId, Guid OrderId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
