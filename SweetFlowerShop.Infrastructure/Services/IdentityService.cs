using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SweetFlowerShop.Application.Common;
using SweetFlowerShop.Application.Features.Auth.Common;
using SweetFlowerShop.Application.Interfaces;
using SweetFlowerShop.Infrastructure.Authentication;
using SweetFlowerShop.Infrastructure.Identity;

namespace SweetFlowerShop.Infrastructure.Services;

internal sealed class IdentityService(
    UserManager<ApplicationUser> userManager,
    IJwtTokenService jwtTokenService,
    IOptions<JwtSettings> jwtSettings) : IIdentityService
{
    public async Task<Result<AuthResponse>> RegisterAsync(
        string email, string password, string firstName, string lastName, CancellationToken ct = default)
    {
        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser is not null)
            return Result<AuthResponse>.Failure("A user with this email already exists.");

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            return Result<AuthResponse>.Failure(result.Errors.Select(e => e.Description));

        await userManager.AddToRoleAsync(user, "Customer");

        return GenerateAuthResponse(user, ["Customer"]);
    }

    public async Task<Result<AuthResponse>> LoginAsync(string email, string password, CancellationToken ct = default)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
            return Result<AuthResponse>.Failure("Invalid email or password.");

        var validPassword = await userManager.CheckPasswordAsync(user, password);
        if (!validPassword)
            return Result<AuthResponse>.Failure("Invalid email or password.");

        var roles = await userManager.GetRolesAsync(user);
        return GenerateAuthResponse(user, roles);
    }

    private Result<AuthResponse> GenerateAuthResponse(ApplicationUser user, IEnumerable<string> roles)
    {
        var token = jwtTokenService.GenerateToken(user.Id, user.Email!, roles);
        var expiresAt = DateTime.UtcNow.AddMinutes(jwtSettings.Value.ExpirationInMinutes);

        return Result<AuthResponse>.Success(new AuthResponse(
            user.Id,
            user.Email!,
            token,
            expiresAt));
    }
}
