namespace SweetFlowerShop.Domain.ValueObjects;

/// <summary>
/// Authoritative product data captured when an order is placed.
/// The application layer builds these snapshots from server-side Product data.
/// </summary>
public sealed record OrderLineSnapshot(
    Guid ProductId,
    string ProductName,
    Money UnitPrice,
    int Quantity,
    string? Notes = null);
