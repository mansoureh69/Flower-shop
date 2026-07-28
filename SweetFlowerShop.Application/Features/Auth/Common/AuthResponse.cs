namespace SweetFlowerShop.Application.Features.Auth.Common;

public record AuthResponse(
    Guid UserId,
    string Email,
    string Token,
    DateTime ExpiresAtUtc);
