using SweetFlowerShop.Domain.Common;

namespace SweetFlowerShop.Domain.Events;

public record OrderCancelledEvent(Guid OrderId, string Reason) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
