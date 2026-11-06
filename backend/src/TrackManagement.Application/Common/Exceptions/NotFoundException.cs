namespace TrackManagement.Application.Common.Exceptions;

/// <summary>The requested resource does not exist. The API maps it to 404 Not Found.</summary>
public class NotFoundException : Exception
{
    public NotFoundException(string resource, object key)
        : base($"{resource} with id '{key}' was not found.")
    {
    }
}
