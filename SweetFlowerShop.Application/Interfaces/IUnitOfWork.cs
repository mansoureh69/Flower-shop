namespace SweetFlowerShop.Application.Interfaces;

/// <summary>
/// Coordinates persistence across aggregate boundaries.
/// Dispatches domain events after successful commit.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a multi-step operation within a single database transaction.
    /// Use when coordinating writes across multiple aggregates.
    /// </summary>
    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default);
}
