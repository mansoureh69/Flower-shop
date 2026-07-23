using SweetFlowerShop.Domain.Common;

namespace SweetFlowerShop.Domain.Events;

public record OrderPlacedEvent(Guid OrderId, Guid CustomerId, decimal TotalAmount) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
