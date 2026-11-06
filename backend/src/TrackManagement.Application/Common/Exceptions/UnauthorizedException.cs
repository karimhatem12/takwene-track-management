namespace TrackManagement.Application.Common.Exceptions;

/// <summary>Credentials were missing or wrong. The API maps it to 401 Unauthorized.</summary>
public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) : base(message)
    {
    }
}
