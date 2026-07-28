using Microsoft.Extensions.Logging;
using SweetFlowerShop.Application.Interfaces;
using SweetFlowerShop.Domain.Common;

namespace SweetFlowerShop.Infrastructure.Services;

/// <summary>
/// Logs domain events for observability.
/// Replace with MediatR IPublisher or message bus integration in production.
/// </summary>
internal sealed class DomainEventDispatcher(ILogger<DomainEventDispatcher> logger) : IDomainEventDispatcher
{
    public Task DispatchAsync(IReadOnlyCollection<IDomainEvent> events, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in events)
        {
            logger.LogInformation(
                "Domain event dispatched: {EventType} at {OccurredOn}",
                domainEvent.GetType().Name,
                domainEvent.OccurredOn);
        }

        return Task.CompletedTask;
    }
}
