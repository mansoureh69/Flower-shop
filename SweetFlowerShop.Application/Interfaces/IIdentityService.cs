using SweetFlowerShop.Application.Common;
using SweetFlowerShop.Application.Features.Auth.Common;

namespace SweetFlowerShop.Application.Interfaces;

/// <summary>
/// Abstracts Identity operations (UserManager) for the Application layer.
/// Implemented in Infrastructure since it depends on ASP.NET Identity.
/// </summary>
public interface IIdentityService
{
    Task<Result<AuthResponse>> RegisterAsync(string email, string password, string firstName, string lastName, CancellationToken ct = default);
    Task<Result<AuthResponse>> LoginAsync(string email, string password, CancellationToken ct = default);
}
