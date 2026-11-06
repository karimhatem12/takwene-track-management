using TrackManagement.Domain.Entities;

namespace TrackManagement.Application.Common.Interfaces;

/// <summary>Creates signed JWT access tokens. Implemented in Infrastructure.</summary>
public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAtUtc) Generate(User user);
}
