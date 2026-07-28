using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SweetFlowerShop.Domain.Common;

namespace SweetFlowerShop.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Intercepts delete operations on ISoftDeletable entities and converts them
/// to soft deletes (UPDATE IsDeleted = true) instead of physical DELETEs.
///
/// WHY THIS INTERCEPTOR:
/// Without it, calling context.Remove(product) generates:
///     DELETE FROM "Products" WHERE "Id" = @id
/// 
/// With it, the same call generates:
///     UPDATE "Products" SET "IsDeleted" = true, "DeletedAtUtc" = @now WHERE "Id" = @id
///
/// HOW IT WORKS:
/// 1. Before SaveChanges, scans all entries with State == Deleted
/// 2. For ISoftDeletable entities, changes State to Modified
/// 3. Sets IsDeleted = true and DeletedAtUtc = now
/// 4. EF Core then generates UPDATE instead of DELETE
///
/// ORDERING: Must run BEFORE AuditableEntityInterceptor so that the Modified state
/// is detected by the audit interceptor (which sets ModifiedBy/ModifiedAtUtc).
///
/// LIFETIME: Scoped — aligns with DbContext lifetime.
/// THREAD SAFETY: Not required (scoped = single request).
/// </summary>
public sealed class SoftDeleteInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ConvertDeletestoSoftDeletes(eventData.Context!);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ConvertDeletestoSoftDeletes(eventData.Context!);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void ConvertDeletestoSoftDeletes(DbContext context)
    {
        var utcNow = DateTime.UtcNow;

        foreach (var entry in context.ChangeTracker.Entries<ISoftDeletable>())
        {
            if (entry.State != EntityState.Deleted)
                continue;

            // Convert physical delete to soft delete
            entry.State = EntityState.Modified;

            // Call the domain method (respects domain logic, e.g., Product.Deactivate())
            entry.Entity.MarkAsDeleted();
        }
    }
}
