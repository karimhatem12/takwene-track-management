namespace TrackManagement.Application.Common.Interfaces;

/// <summary>
/// The user making the current request, read from the JWT. Implemented in the API layer.
/// Both values are null when there is no signed-in user (e.g. startup seeding, register).
/// </summary>
public interface ICurrentUserService
{
    int? UserId { get; }
    string? Email { get; }
}
