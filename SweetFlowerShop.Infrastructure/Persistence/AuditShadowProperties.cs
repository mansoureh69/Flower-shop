namespace SweetFlowerShop.Infrastructure.Persistence;

/// <summary>
/// Constants for audit shadow property names.
/// Using constants prevents magic strings scattered across interceptors, configurations, and queries.
/// </summary>
public static class AuditShadowProperties
{
    public const string CreatedAtUtc = nameof(CreatedAtUtc);
    public const string ModifiedAtUtc = nameof(ModifiedAtUtc);
    public const string CreatedBy = nameof(CreatedBy);
    public const string ModifiedBy = nameof(ModifiedBy);
}
