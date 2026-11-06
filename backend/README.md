# Track Management API (.NET 10)

The back end of Takwene Track Management: an ASP.NET Core Web API using Clean Architecture,
EF Core and SQL Server LocalDB. Prerequisites, getting a JWT, the full endpoint list and troubleshooting
are in the [main README](../README.md).

```bash
dotnet run --project src/TrackManagement.Api --launch-profile https   # https://localhost:7142/swagger
dotnet test                                                            # 57 unit tests
```

In Development the API creates and migrates the `TakweneTracks` LocalDB database, seeds sample data and
creates a demo user on startup. To apply migrations by hand:

```bash
dotnet tool restore
dotnet ef database update --project src/TrackManagement.Infrastructure --startup-project src/TrackManagement.Api
```

## Projects

Dependencies point inwards: `Api → Application → Domain`, and `Infrastructure` implements the
interfaces that `Application` defines.

| Project | Holds | Depends on |
|---|---|---|
| `TrackManagement.Domain` | Entities with their business rules (status flow, distribution rules, edit locks, genre list), enums, `DomainException` | nothing |
| `TrackManagement.Application` | Use cases per feature (`Artists`, `Tracks`, `Dsps`, `Auth`, `Audit`): services, DTOs, FluentValidation validators; interfaces in `Common/Interfaces` | Domain |
| `TrackManagement.Infrastructure` | EF Core `ApplicationDbContext`, entity configurations, migrations, seed data, audit interceptor, JWT generator, password hashing | Application |
| `TrackManagement.Api` | Controllers, JWT/CORS/rate-limit setup, global exception handler (ProblemDetails), Swagger | Application, Infrastructure |
| `tests/TrackManagement.Tests` | xUnit tests for domain rules, validators and the audit interceptor | Domain, Application, Infrastructure |

## Where to find things

| Looking for… | Go to |
|---|---|
| Business rules (status, distribute, what can be edited) | `src/TrackManagement.Domain/Entities/Track.cs` |
| Allowed genres | `src/TrackManagement.Domain/Common/Genres.cs` |
| Input validation rules | `src/TrackManagement.Application/*/…Validator(s).cs` |
| How errors become HTTP responses | `src/TrackManagement.Api/Middleware/GlobalExceptionHandler.cs` |
| Auth, CORS, rate limiting, Swagger setup | `src/TrackManagement.Api/Program.cs` |
| Audit trail (who changed what) | `src/TrackManagement.Infrastructure/Persistence/Interceptors/AuditInterceptor.cs` |
| Tables, keys and indexes | `src/TrackManagement.Infrastructure/Persistence/Configurations/` |
| Sample data | `src/TrackManagement.Infrastructure/Persistence/Seed/SampleData.cs` |
| Demo user (Development only) | `src/TrackManagement.Infrastructure/Persistence/Seed/DevelopmentDataSeeder.cs` |

## Request flow

```
HTTP request
  → JWT check (every endpoint except /api/auth/*)
  → Controller                 thin: calls one service method
  → Application service        validates input (400), checks the database (404 / 409)
  → Domain entity method       enforces business rules (409)
  → ApplicationDbContext       SaveChanges; the audit interceptor records the change
  → DTO back to the client     errors become RFC 7807 ProblemDetails
```
