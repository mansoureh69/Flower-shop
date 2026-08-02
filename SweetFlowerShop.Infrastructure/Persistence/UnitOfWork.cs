using Microsoft.EntityFrameworkCore;
using SweetFlowerShop.Application.Interfaces;
using SweetFlowerShop.Domain.Common;
using System.Text.Json;

namespace SweetFlowerShop.Infrastructure.Persistence;

/// <summary>
/// Orchestrates the SaveChanges pipeline:
/// 1. Collect domain events from tracked aggregates
/// 2. Clear events (prevent re-dispatch on retry)
/// 3. Persist changes (interceptors fire here)
/// 4. Dispatch domain events (only after successful commit)
/// </summary>
public sealed class UnitOfWork(
    FlowerShopDbContext context) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Step 1 & 2: Harvest and clear domain events BEFORE save
        var aggregates = GetAggregatesWithDomainEvents();
        AddOutboxMessages(aggregates.SelectMany(a => a.DomainEvents));

        // Step 3: Persist — interceptors (SoftDelete → Audit) fire inside
        var result = await context.SaveChangesAsync(cancellationToken);
        aggregates.ForEach(a => a.ClearDomainEvents());

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
            var aggregates = GetAggregatesWithDomainEvents();
            AddOutboxMessages(aggregates.SelectMany(a => a.DomainEvents));
            await context.SaveChangesAsync(ct);

            await transaction.CommitAsync(ct);

            aggregates.ForEach(a => a.ClearDomainEvents());
        }, cancellationToken);
    }

    private List<AggregateRoot> GetAggregatesWithDomainEvents()
    {
        var aggregates = context.ChangeTracker
            .Entries<AggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        return aggregates;
    }

    private void AddOutboxMessages(IEnumerable<IDomainEvent> domainEvents)
    {
        foreach (var domainEvent in domainEvents)
        {
            var type = domainEvent.GetType();
            context.OutboxMessages.Add(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                OccurredOnUtc = domainEvent.OccurredOn,
                Type = type.AssemblyQualifiedName!,
                Payload = JsonSerializer.Serialize(domainEvent, type)
            });
        }
    }
}
