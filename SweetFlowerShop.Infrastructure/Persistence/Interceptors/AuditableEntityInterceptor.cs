using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SweetFlowerShop.Application.Interfaces;
using SweetFlowerShop.Domain.Common;

namespace SweetFlowerShop.Infrastructure.Persistence.Interceptors;

/// <summary>
/// EF Core SaveChanges interceptor that automatically populates audit shadow properties.
///
/// HOW IT WORKS:
/// 1. Before SaveChanges commits, this interceptor fires
/// 2. It scans all tracked entities that implement IAuditable
/// 3. For Added entities: sets CreatedAtUtc and CreatedBy
/// 4. For Modified entities: sets ModifiedAtUtc and ModifiedBy
///
/// WHY AN INTERCEPTOR (not overriding SaveChanges):
/// - Interceptors are composable (multiple can chain)
/// - They don't require inheriting DbContext (cleaner with sealed DbContext)
/// - They can be registered via DI (testable)
/// - They work with both SaveChanges and SaveChangesAsync
///
/// SHADOW PROPERTIES ACCESSED:
/// - "CreatedAtUtc" (DateTime) — set once
/// - "ModifiedAtUtc" (DateTime?) — set on every update
/// - "CreatedBy" (string?) — user who created
/// - "ModifiedBy" (string?) — user who modified
///
/// LIFETIME: Singleton — interceptors must be thread-safe.
/// This one is safe because ICurrentUserService is resolved per-call from the DbContext's service provider.
/// </summary>
public sealed class AuditableEntityInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;

    public AuditableEntityInterceptor(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ApplyAuditInfo(eventData.Context!);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ApplyAuditInfo(eventData.Context!);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ApplyAuditInfo(DbContext context)
    {
        var utcNow = DateTime.UtcNow;
        var userName = _currentUserService.UserName ?? "system";

        foreach (var entry in context.ChangeTracker.Entries<IAuditable>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Property(AuditShadowProperties.CreatedAtUtc).CurrentValue = utcNow;
                    entry.Property(AuditShadowProperties.CreatedBy).CurrentValue = userName;
                    break;

                case EntityState.Modified:
                    entry.Property(AuditShadowProperties.ModifiedAtUtc).CurrentValue = utcNow;
                    entry.Property(AuditShadowProperties.ModifiedBy).CurrentValue = userName;
                    break;
            }
        }
    }
}
