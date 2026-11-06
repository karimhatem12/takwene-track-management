using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TrackManagement.Api.Middleware;
using TrackManagement.Api.Security;
using TrackManagement.Api.Services;
using TrackManagement.Application;
using TrackManagement.Application.Common.Interfaces;
using TrackManagement.Infrastructure;
using TrackManagement.Infrastructure.Authentication;
using TrackManagement.Infrastructure.Persistence.Seed;

var builder = WebApplication.CreateBuilder(args);

// Application + Infrastructure layers (services, validators, EF Core, JWT, password hashing).
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// The signed-in user (from the JWT), used by the audit trail to record who made each change.
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        // Enums travel as text: "status": "Draft" instead of "status": 0.
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Errors are returned as RFC 7807 ProblemDetails.
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// ---- Authentication: JWT bearer tokens issued by POST /api/auth/login ----
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtSettings>>((options, jwtOptions) =>
    {
        var jwt = jwtOptions.Value;
        options.MapInboundClaims = false; // keep "sub", "email", "name" as they are in the token
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = JwtTokenGenerator.CreateSigningKey(jwt.SigningKey),
            ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 }, // reject "alg: none" and algorithm swaps
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = "name"
        };
    });

// ---- Authorization: secure by default ----
// Every endpoint requires a valid token unless it is explicitly marked [AllowAnonymous]
// (only register and login). A new endpoint can't be left unprotected by forgetting [Authorize].
builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

// ---- CORS: only the Angular front-end origin(s) from configuration ----
const string FrontendCorsPolicy = "Frontend";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
    options.AddPolicy(FrontendCorsPolicy, policy =>
        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

// ---- Rate limiting: max 10 login/register attempts per minute per IP ----
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.Headers.RetryAfter = "60";
        await context.HttpContext.RequestServices.GetRequiredService<IProblemDetailsService>().WriteAsync(new ProblemDetailsContext
        {
            HttpContext = context.HttpContext,
            ProblemDetails =
            {
                Status = StatusCodes.Status429TooManyRequests,
                Title = "Too many requests.",
                Detail = "Too many sign-in attempts. Please wait a minute and try again."
            }
        });
    };
    options.AddPolicy(RateLimitPolicies.Auth, httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1) }));
});

// OpenAPI document at /openapi/v1.json, browsed with Swagger UI at /swagger (with an Authorize button).
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
    options.AddOperationTransformer<BearerSecurityRequirementTransformer>();
});

var app = builder.Build();

app.UseExceptionHandler();
// Gives empty error responses (401 from JWT, 403, 429...) a ProblemDetails body too.
app.UseStatusCodePages();

// Must run before Swagger UI: otherwise the Swagger page is served over http while
// /openapi/v1.json is redirected to https (another origin) and the browser blocks it.
app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Track Management API v1");
        options.DocumentTitle = "Takwene Track Management API";
        options.EnablePersistAuthorization(); // keep the token after a page refresh (dev only)
    });

    // Opening the bare URL (e.g. https://localhost:7142/) goes straight to Swagger.
    app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription().AllowAnonymous();

    // Apply migrations and create the demo user (Development only, see DevelopmentDataSeeder).
    await app.Services.SeedDevelopmentDataAsync();
}

app.UseCors(FrontendCorsPolicy);
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
