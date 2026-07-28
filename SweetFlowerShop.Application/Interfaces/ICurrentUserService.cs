namespace SweetFlowerShop.Application.Interfaces;

/// <summary>
/// Provides the identity of the currently authenticated user.
/// 
/// WHY IN APPLICATION (not Domain):
/// The current user is a request-scoped concern. Domain doesn't know about HTTP.
/// Application defines the contract; Infrastructure implements it (from HttpContext).
///
/// USED BY: Audit interceptor to populate "CreatedBy" / "ModifiedBy" shadow properties.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// The authenticated user's ID (null if anonymous).
    /// </summary>
    Guid? UserId { get; }

    /// <summary>
    /// The authenticated user's email or username (null if anonymous).
    /// </summary>
    string? UserName { get; }
}
