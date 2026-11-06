using Microsoft.AspNetCore.Identity;
using TrackManagement.Application.Common.Interfaces;
using TrackManagement.Domain.Entities;

namespace TrackManagement.Infrastructure.Authentication;

/// <summary>
/// Wraps ASP.NET Core Identity's PasswordHasher: PBKDF2 with HMAC-SHA512,
/// 100,000 iterations and a random salt per password (Identity V3 format).
/// We never write our own crypto.
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<User> _inner = new();

    public string Hash(string password) => _inner.HashPassword(null!, password);

    public bool Verify(string passwordHash, string password) =>
        _inner.VerifyHashedPassword(null!, passwordHash, password) != PasswordVerificationResult.Failed;
}
