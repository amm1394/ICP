using Core.Icp.Application;
using Infrastructure.Data;
using Infrastructure.Icp.Files;
using Presentation.Icp.API.Middleware;
using System.Text.Json.Serialization;


// Program bootstrap for the ICP Analysis API.
// This file wires up dependency injection, configures JSON, Swagger, CORS, and the HTTP middleware pipeline.
// Note: User-facing API messages (success/error) are localized in Persian elsewhere (controllers, ApiResponse, middleware).
var builder = WebApplication.CreateBuilder(args);

#region Services Configuration

// Register MVC controllers and configure System.Text.Json options
// - ReferenceHandler.IgnoreCycles: Avoid circular reference serialization (e.g., EF navigation properties)
// - DefaultIgnoreCondition.WhenWritingNull: Do not output null properties (smaller, cleaner payloads)
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// Register Swagger/OpenAPI for interactive documentation and testing
// Enabled only in Development in the pipeline below
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "ICP Analysis API",
        Version = "v1",
        Description = "API for ICP-MS Data Analysis System",
        Contact = new()
        {
            Name = "Your Name",
            Email = "your.email@example.com"
        }
    });
});

// Register Infrastructure Data Layer (DbContext, repositories, Unit of Work)
// The DI extension configures the SQL Server provider and migrations assembly
// Register Infrastructure Data Layer (DbContext, repositories, Unit of Work)
builder.Services.AddInfrastructureData(builder.Configuration);

// Register Files layer (پردازش CSV/Excel)
builder.Services.AddInfrastructureFiles();

// Register Application layer (سرویس‌های دامنه / use case ها)
builder.Services.AddApplication();

builder.Services.AddHealthChecks();

// Configure CORS policies
// - "AllowAll": Development-friendly policy (any origin/method/header). Do NOT use in production.
// - "AllowSpecific": Production-ready policy using configured allowed origins (appsettings: Cors:AllowedOrigins)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });

    options.AddPolicy("AllowSpecific", policy =>
    {
        // Example configuration in appsettings.json:
        // "Cors": { "AllowedOrigins": [ "https://app.example.com", "https://admin.example.com" ] }
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? new[] { "http://localhost:5000" };

        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Health Checks (optional)
// Add built-in health checks when you need dependency health reporting (DB, external services, etc.)
//builder.Services.AddHealthChecks()
//    .AddDbContextCheck<Infrastructure.Icp.Data.Context.ICPDbContext>();

#endregion

var app = builder.Build();

#region Middleware Pipeline

// Development-only Swagger UI (interactive docs)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ICP Analysis API V1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at the application root ("/")
    });
}

// Global error handler middleware
// Ensures all errors produce a consistent JSON body with Persian messages where applicable
app.UseMiddleware<ErrorHandlerMiddleware>();

// Redirect HTTP to HTTPS if configured (recommended in production)
app.UseHttpsRedirection();

// CORS policy
// Development: open policy for easy testing
// Production: switch to "AllowSpecific" and configure allowed origins
app.UseCors("AllowAll");
// app.UseCors("AllowSpecific");

// Authorization (add Authentication above if needed, e.g., JWT/Identity)
app.UseAuthorization();

// Attribute-routed API controllers
app.MapControllers();

// Lightweight health check endpoint (returns 200/503). Pair with AddHealthChecks to report dependencies.
app.MapHealthChecks("/health");

#endregion

#region Database Migration (Optional - for Development)

// Validate database connectivity and optionally apply migrations during development
// Persian user-facing messages remain defined in ApiResponse/Error middleware; logs are for operators/devs.
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<Infrastructure.Icp.Data.Context.ICPDbContext>();
        // context.Database.Migrate(); // Uncomment to auto-apply EF Core migrations at startup (dev only)
        app.Logger.LogInformation("Database connection successful");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "An error occurred while migrating the database");
    }
}

#endregion

// Start the web application
app.Run();