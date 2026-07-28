using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SweetFlowerShop.Application.Interfaces;

namespace SweetFlowerShop.Infrastructure.Services;

/// <summary>
/// Resolves the current user from HttpContext.
/// 
/// LIFETIME: Scoped — one instance per HTTP request.
/// THREAD SAFETY: Not required (scoped = single request = single thread in ASP.NET Core).
/// 
/// WHY NOT SINGLETON:
/// HttpContext is request-scoped. Capturing it in a singleton creates race conditions
/// and stale data. IHttpContextAccessor is singleton-safe, but we resolve values per-request.
/// </summary>
public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var sub = _httpContextAccessor.HttpContext?.User
                .FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(sub, out var id) ? id : null;
        }
    }

    public string? UserName =>
        _httpContextAccessor.HttpContext?.User
            .FindFirstValue(ClaimTypes.Email)
        ?? _httpContextAccessor.HttpContext?.User
            .FindFirstValue(ClaimTypes.Name);
}
