using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace TrackManagement.Infrastructure.Persistence.Converters;

/// <summary>
/// SQL Server's datetime2 does not store a time zone, so EF Core reads values back with
/// DateTimeKind.Unspecified and the JSON goes out as "2024-03-01T09:00:00" (no "Z").
/// A browser would then treat it as local time. We always save UTC, so mark values
/// read from the database as UTC and the JSON becomes "2024-03-01T09:00:00Z".
/// </summary>
public class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
{
    public UtcDateTimeConverter()
        : base(
            toDb => toDb.Kind == DateTimeKind.Utc ? toDb : toDb.ToUniversalTime(),
            fromDb => DateTime.SpecifyKind(fromDb, DateTimeKind.Utc))
    {
    }
}
