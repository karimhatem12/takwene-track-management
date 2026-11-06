using System.Text.Json.Serialization;
using TrackManagement.Api.Middleware;
using TrackManagement.Application;
using TrackManagement.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Application + Infrastructure layers (services, validators, EF Core).
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        // Enums travel as text: "status": "Draft" instead of "status": 0.
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Errors are returned as RFC 7807 ProblemDetails.
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// OpenAPI document at /openapi/v1.json, browsed with Swagger UI at /swagger.
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();

// Must run before Swagger UI: otherwise the Swagger page is served over http while
// /openapi/v1.json is redirected to https (another origin) and the browser blocks it.
app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Track Management API v1");
        options.DocumentTitle = "Takwene Track Management API";
    });

    // Opening the bare URL (e.g. https://localhost:7142/) goes straight to Swagger.
    app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
