namespace TrackManagement.Domain.Exceptions;

/// <summary>
/// Thrown when an operation breaks a business rule
/// (e.g. an invalid status transition or a duplicate DSP submission).
/// The API maps it to 409 Conflict.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}
