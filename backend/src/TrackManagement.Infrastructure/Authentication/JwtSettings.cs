using System.ComponentModel.DataAnnotations;

namespace TrackManagement.Infrastructure.Authentication;

/// <summary>
/// Bound from the "Jwt" section. The signing key is NOT stored in appsettings.json:
/// Development reads it from appsettings.Development.json, any other environment
/// must provide it (e.g. environment variable Jwt__SigningKey). The app refuses to
/// start without a key of at least 32 characters (256 bits for HMAC-SHA256).
/// </summary>
public class JwtSettings
{
    public const string SectionName = "Jwt";

    [Required]
    public string Issuer { get; init; } = string.Empty;

    [Required]
    public string Audience { get; init; } = string.Empty;

    [Range(1, 1440)]
    public int ExpiryMinutes { get; init; } = 60;

    [Required(ErrorMessage = "Jwt:SigningKey is missing. Set it via configuration (e.g. env var Jwt__SigningKey).")]
    [MinLength(32, ErrorMessage = "Jwt:SigningKey must be at least 32 characters.")]
    public string SigningKey { get; init; } = string.Empty;
}
