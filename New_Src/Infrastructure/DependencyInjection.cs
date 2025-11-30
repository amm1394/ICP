using Application.Services;
using Infrastructure.Persistence;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

/// <summary>
/// Registers infrastructure services, persistence and hosted/background services.
/// Keep registrations here minimal and infrastructure-specific.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<IsatisDbContext>(options => options.UseSqlServer(connectionString));

        // Persistence implementations
        // Use fully-qualified interface/implementation types to avoid ambiguous-reference errors
        services.AddScoped<Application.Services.IProjectPersistenceService, Infrastructure.Services.ProjectPersistenceService>();

        // Import service (scoped because it uses DbContext)
        services.AddScoped<Application.Services.IImportService, Infrastructure.Services.ImportService>();

        // Background import queue - singleton hosted service that creates scopes for scoped services
        // Register concrete background worker as singleton and expose it as the application-facing IImportQueueService
        services.AddSingleton<Infrastructure.Services.BackgroundImportQueueService>();
        services.AddSingleton<Application.Services.IImportQueueService>(sp => sp.GetRequiredService<Infrastructure.Services.BackgroundImportQueueService>());
        services.AddHostedService(sp => sp.GetRequiredService<Infrastructure.Services.BackgroundImportQueueService>());

        // Processing services and processors (scoped)
        services.AddScoped<Application.Services.IProcessingService, Infrastructure.Services.ProcessingService>();
        services.AddScoped<Application.Services.IRowProcessor, Infrastructure.Services.Processors.ComputeStatisticsProcessor>();

        // Cleanup hosted service for old jobs/temp files
        services.AddSingleton<Infrastructure.Services.CleanupHostedService>();
        services.AddHostedService(sp => sp.GetRequiredService<Infrastructure.Services.CleanupHostedService>());

        return services;
    }
}