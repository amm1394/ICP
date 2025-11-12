using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Presentation.Icp.API.Middleware;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

#region Services Configuration

// Add Controllers with JSON options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// Add Swagger/OpenAPI
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

// Add Infrastructure Data Layer
builder.Services.AddInfrastructureData(builder.Configuration);

// Add CORS
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
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? new[] { "http://localhost:5000" };

        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Add Health Checks
//builder.Services.AddHealthChecks()
//    .AddDbContextCheck<Infrastructure.Icp.Data.Context.ICPDbContext>();

#endregion

var app = builder.Build();

#region Middleware Pipeline

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ICP Analysis API V1");
        c.RoutePrefix = string.Empty; // Swagger در root
    });
}

// Global Error Handler
app.UseMiddleware<ErrorHandlerMiddleware>();

app.UseHttpsRedirection();

// CORS
app.UseCors("AllowAll"); // برای Development
// app.UseCors("AllowSpecific"); // برای Production

app.UseAuthorization();

app.MapControllers();

// Health Check Endpoint
app.MapHealthChecks("/health");

#endregion

#region Database Migration (Optional - for Development)

// Auto-migrate database in development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<Infrastructure.Icp.Data.Context.ICPDbContext>();
        // context.Database.Migrate(); // فعلاً comment کن، بعداً باز می‌کنیم
        app.Logger.LogInformation("Database connection successful");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "An error occurred while migrating the database");
    }
}

#endregion

app.Run();