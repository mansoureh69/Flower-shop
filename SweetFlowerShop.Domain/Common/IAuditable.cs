namespace SweetFlowerShop.Domain.Common;

/// <summary>
/// Marker interface for entities that should be automatically audited.
/// 
/// WHY A MARKER (no properties):
/// Unlike ISoftDeletable (which has properties the domain uses), audit data is PURELY
/// an infrastructure concern. The domain entity doesn't need to read or write CreatedBy/ModifiedBy.
/// Therefore, we use a marker interface — no properties leak into the domain.
///
/// WHAT HAPPENS:
/// Infrastructure detects entities implementing this interface and adds Shadow Properties:
/// - "CreatedAtUtc" (DateTime) — set once on insert
/// - "ModifiedAtUtc" (DateTime?) — set on every update
/// - "CreatedBy" (string?) — user who created the record
/// - "ModifiedBy" (string?) — user who last modified the record
///
/// These exist ONLY as database columns, managed by an EF Core interceptor.
/// </summary>
public interface IAuditable;
