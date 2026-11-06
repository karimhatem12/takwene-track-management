namespace TrackManagement.Api.Security;

public static class RateLimitPolicies
{
    /// <summary>Applied to login/register to slow down password guessing.</summary>
    public const string Auth = "auth";
}
