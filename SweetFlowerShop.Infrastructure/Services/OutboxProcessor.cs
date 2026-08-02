using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SweetFlowerShop.Application.Interfaces;
using SweetFlowerShop.Domain.Common;
using SweetFlowerShop.Infrastructure.Persistence;

namespace SweetFlowerShop.Infrastructure.Services;

internal sealed class OutboxProcessor(
    IServiceScopeFactory scopeFactory,
    ILogger<OutboxProcessor> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try { await ProcessBatch(stoppingToken); }
            catch (Exception exception) { logger.LogError(exception, "Outbox processing failed."); }
        }
    }

    private async Task ProcessBatch(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<FlowerShopDbContext>();
        var dispatcher = scope.ServiceProvider.GetRequiredService<IDomainEventDispatcher>();
        var messages = await context.OutboxMessages
            .Where(x => x.ProcessedOnUtc == null)
            .OrderBy(x => x.OccurredOnUtc)
            .Take(50)
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                var type = Type.GetType(message.Type, throwOnError: true)!;
                var domainEvent = (IDomainEvent)JsonSerializer.Deserialize(message.Payload, type)!;
                await dispatcher.DispatchAsync([domainEvent], cancellationToken);
                message.ProcessedOnUtc = DateTime.UtcNow;
                message.Error = null;
            }
            catch (Exception exception)
            {
                message.Error = exception.Message;
                logger.LogError(exception, "Outbox message {MessageId} failed.", message.Id);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
