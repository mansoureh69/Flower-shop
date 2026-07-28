using System.Data.Common;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace SweetFlowerShop.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Monitors command execution duration and logs slow queries.
///
/// WHY THIS INTERCEPTOR:
/// Slow queries are the #1 production performance issue. Without monitoring,
/// they go unnoticed until users complain. This interceptor provides:
/// - Warning logs for queries exceeding threshold (default: 200ms)
/// - The exact SQL text for investigation
/// - Duration measurement for dashboards/alerting
///
/// HOW IT HOOKS IN:
/// Uses DbCommandInterceptor to wrap around the actual SQL execution:
///   CommandExecuting → [database executes query] → CommandExecuted
///   The elapsed time between these two points = query duration.
///
/// LIFETIME: Singleton — stateless, thread-safe (only reads config + writes logs).
/// 
/// PRODUCTION USE:
/// - Feed slow query logs into Application Insights / Datadog / Grafana
/// - Alert when P95 query time exceeds threshold
/// - Use tagged queries to identify which business operation caused it
/// </summary>
public sealed class SlowQueryInterceptor : DbCommandInterceptor
{
    private readonly ILogger<SlowQueryInterceptor> _logger;
    private readonly TimeSpan _threshold;

    public SlowQueryInterceptor(ILogger<SlowQueryInterceptor> logger)
    {
        _logger = logger;
        _threshold = TimeSpan.FromMilliseconds(200); // Configurable via Options pattern if needed
    }

    public override DbDataReader ReaderExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result)
    {
        LogIfSlow(command, eventData.Duration);
        return base.ReaderExecuted(command, eventData, result);
    }

    public override ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result,
        CancellationToken cancellationToken = default)
    {
        LogIfSlow(command, eventData.Duration);
        return base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override int NonQueryExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        int result)
    {
        LogIfSlow(command, eventData.Duration);
        return base.NonQueryExecuted(command, eventData, result);
    }

    public override ValueTask<int> NonQueryExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        LogIfSlow(command, eventData.Duration);
        return base.NonQueryExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override object? ScalarExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        object? result)
    {
        LogIfSlow(command, eventData.Duration);
        return base.ScalarExecuted(command, eventData, result);
    }

    public override ValueTask<object?> ScalarExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        object? result,
        CancellationToken cancellationToken = default)
    {
        LogIfSlow(command, eventData.Duration);
        return base.ScalarExecutedAsync(command, eventData, result, cancellationToken);
    }

    private void LogIfSlow(DbCommand command, TimeSpan duration)
    {
        if (duration <= _threshold)
            return;

        _logger.LogWarning(
            "SLOW QUERY detected ({Duration}ms). Threshold: {Threshold}ms. SQL: {Sql}",
            duration.TotalMilliseconds,
            _threshold.TotalMilliseconds,
            command.CommandText);
    }
}
