using TrackManagement.Domain.Exceptions;

namespace TrackManagement.Domain.Entities;

/// <summary>
/// A label-team operator who signs in to manage tracks. Not the same thing as an Artist.
/// </summary>
public class User
{
    public int Id { get; private set; }
    public string FullName { get; private set; } = null!;

    /// <summary>Stored lower-case; unique across users.</summary>
    public string Email { get; private set; } = null!;

    /// <summary>Hashed password. The plain-text password never reaches the domain.</summary>
    public string PasswordHash { get; private set; } = null!;

    public DateTime CreatedAt { get; private set; }

    // Required by EF Core.
    private User()
    {
    }

    public static User Create(string fullName, string email, string passwordHash, DateTime createdAtUtc)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new DomainException("Full name is required.");
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email is required.");
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("Password hash is required.");

        return new User
        {
            FullName = fullName.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            CreatedAt = createdAtUtc
        };
    }
}
