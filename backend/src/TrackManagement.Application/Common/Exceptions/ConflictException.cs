namespace TrackManagement.Application.Common.Exceptions;

/// <summary>
/// The request clashes with existing data (e.g. an email or ISRC already in use).
/// The API maps it to 409 Conflict.
/// </summary>
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message)
    {
    }
}
