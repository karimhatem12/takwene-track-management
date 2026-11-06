using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TrackManagement.Application.Common.Interfaces;
using TrackManagement.Domain.Entities;

namespace TrackManagement.Infrastructure.Persistence.Seed;

/// <summary>
/// Development-only startup tasks. Called from Program.cs only when the environment is Development.
/// The demo user is deliberately NOT part of the HasData seed: migrations run in every environment,
/// and a well-known login must never exist in production.
/// </summary>
public static class DevelopmentDataSeeder
{
    public static async Task SeedDevelopmentDataAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var timeProvider = scope.ServiceProvider.GetRequiredService<TimeProvider>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(DevelopmentDataSeeder));

        // Convenience for local runs: bring the database up to date (same as `dotnet ef database update`).
        await db.Database.MigrateAsync(cancellationToken);

        var section = configuration.GetSection("DemoUser");
        var fullName = section["FullName"];
        var email = section["Email"]?.Trim().ToLowerInvariant();
        var password = section["Password"];

        if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogInformation("No DemoUser configured; skipping demo user.");
            return;
        }

        if (await db.Users.AnyAsync(u => u.Email == email, cancellationToken))
            return;

        db.Users.Add(User.Create(fullName, email, hasher.Hash(password), timeProvider.GetUtcNow().UtcDateTime));
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Created development demo user {Email}.", email);
    }
}
