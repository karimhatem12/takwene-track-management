using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TrackManagement.Application.Artists;
using TrackManagement.Application.Audit;
using TrackManagement.Application.Auth;
using TrackManagement.Application.Common.Interfaces;
using TrackManagement.Application.Dsps;
using TrackManagement.Application.Tracks;

namespace TrackManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Stop at the first failing rule per property, so the client gets one clear
        // message per field (e.g. "ISRC is required." instead of also "ISRC must look like...").
        ValidatorOptions.Global.DefaultRuleLevelCascadeMode = CascadeMode.Stop;
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);

        // Injected instead of DateTime.UtcNow so time can be faked in tests.
        services.AddSingleton(TimeProvider.System);

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IArtistService, ArtistService>();
        services.AddScoped<ITrackService, TrackService>();
        services.AddScoped<IDspService, DspService>();
        services.AddScoped<IAuditLogService, AuditLogService>();

        return services;
    }
}
