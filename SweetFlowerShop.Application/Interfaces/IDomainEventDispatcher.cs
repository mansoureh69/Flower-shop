using SweetFlowerShop.Domain.Common;

namespace SweetFlowerShop.Application.Interfaces;

/// <summary>
/// Dispatches domain events raised by aggregate roots after persistence succeeds.
/// Implement with MediatR, in-process handlers, or a message bus.
/// </summary>
public interface IDomainEventDispatcher
{
    Task DispatchAsync(IReadOnlyCollection<IDomainEvent> events, CancellationToken cancellationToken = default);
}
