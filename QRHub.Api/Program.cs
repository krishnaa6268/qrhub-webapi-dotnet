using QRHub.Api.Extensions;
using QRHub.Api.Middleware;
using QRHub.Application;
using QRHub.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add Clean Architecture Layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Add Controllers and native OpenAPI
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddOpenApi();

// Enable CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Global Exception Handler Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure OpenAPI & Modern Scalar Documentation UI
app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.WithTitle("QRHub API Documentation")
           .WithTheme(ScalarTheme.Moon)
           .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
});

app.UseCors("AllowAll");

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Automatically apply EF Core Migrations to PostgreSQL on startup
app.ApplyMigrations();

// Redirect root to Scalar API Reference UI
app.MapGet("/", () => Results.Redirect("/scalar/v1"));

app.MapControllers();

app.Run();
