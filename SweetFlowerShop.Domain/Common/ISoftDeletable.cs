namespace SweetFlowerShop.Domain.Common;

/// <summary>
/// Marker interface for entities that support soft deletion.
/// 
/// WHY IN DOMAIN:
/// Soft delete is a BUSINESS decision ("we never truly delete customer data" or
/// "deleted products must remain visible in historical orders"). The Domain owns
/// this decision. Infrastructure just implements the persistence mechanism.
///
/// RULES:
/// - Entities implementing this will never be physically deleted from the database
/// - Queries automatically exclude soft-deleted records (via Global Query Filters)
/// - Admin/audit queries can bypass the filter with .IgnoreQueryFilters()
/// </summary>
public interface ISoftDeletable
{
    bool IsDeleted { get; }
    DateTime? DeletedAtUtc { get; }

    /// <summary>
    /// Marks the entity as soft-deleted. Called by the domain (not infrastructure).
    /// </summary>
    void MarkAsDeleted();
}
