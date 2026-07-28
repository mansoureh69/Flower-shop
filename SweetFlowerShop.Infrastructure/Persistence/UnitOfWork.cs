using Microsoft.EntityFrameworkCore;
using SweetFlowerShop.Application.Interfaces;
using SweetFlowerShop.Domain.Common;

namespace SweetFlowerShop.Infrastructure.Persistence;

/// <summary>
/// Orchestrates the SaveChanges pipeline:
/// 1. Collect domain events from tracked aggregates
/// 2. Clear events (prevent re-dispatch on retry)
/// 3. Persist changes (interceptors fire here)
/// 4. Dispatch domain events (only after successful commit)
/// </summary>
public sealed class UnitOfWork(
    FlowerShopDbContext context,
    IDomainEventDispatcher domainEventDispatcher) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Step 1 & 2: Harvest and clear domain events BEFORE save
        var domainEvents = CollectAndClearDomainEvents();

        // Step 3: Persist — interceptors (SoftDelete → Audit) fire inside
        var result = await context.SaveChangesAsync(cancellationToken);

        // Step 4: Dispatch events AFTER successful commit
        if (domainEvents.Count > 0)
        {
            await domainEventDispatcher.DispatchAsync(domainEvents, cancellationToken);
        }

        return result;
    }

    public async Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default)
    {
        var strategy = context.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async ct =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync(ct);

            await operation(ct);
            await SaveChangesAsync(ct);

            await transaction.CommitAsync(ct);
        }, cancellationToken);
    }

    private List<IDomainEvent> CollectAndClearDomainEvents()
    {
        var aggregates = context.ChangeTracker
            .Entries<AggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        var events = aggregates
            .SelectMany(a => a.DomainEvents)
            .ToList();

        aggregates.ForEach(a => a.ClearDomainEvents());

        return events;
    }
}
