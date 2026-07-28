using System.ComponentModel.DataAnnotations;

namespace SweetFlowerShop.Infrastructure.Authentication;

/// <summary>
/// Strongly-typed JWT configuration bound from appsettings.json "JwtSettings" section.
/// Validated at startup via DataAnnotations — app won't start with invalid config.
/// </summary>
public sealed class JwtSettings
{
    public const string SectionName = "JwtSettings";

    [Required(ErrorMessage = "JWT Secret is required.")]
    [MinLength(32, ErrorMessage = "JWT Secret must be at least 32 characters for HMAC-SHA256.")]
    public string Secret { get; init; } = string.Empty;

    [Required(ErrorMessage = "JWT Issuer is required.")]
    public string Issuer { get; init; } = string.Empty;

    [Required(ErrorMessage = "JWT Audience is required.")]
    public string Audience { get; init; } = string.Empty;

    [Range(1, 1440, ErrorMessage = "ExpirationInMinutes must be between 1 and 1440 (24 hours).")]
    public int ExpirationInMinutes { get; init; } = 60;
}
