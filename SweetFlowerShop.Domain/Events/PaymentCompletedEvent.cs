using SweetFlowerShop.Domain.Common;

namespace SweetFlowerShop.Domain.Events;

public record PaymentCompletedEvent(Guid PaymentId, Guid OrderId, decimal Amount) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
