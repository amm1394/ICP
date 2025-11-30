using Application.Services;
using Infrastructure.Persistence;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<IsatisDbContext>(options => options.UseSqlServer(connectionString));

        // Persistence implementations
        services.AddScoped<Application.Services.IProjectPersistenceService, Infrastructure.Services.ProjectPersistenceService>();

        // Import service
        services.AddScoped<Application.Services.IImportService, Infrastructure.Services.ImportService>();

        // Background import queue
        services.AddSingleton<Infrastructure.Services.BackgroundImportQueueService>();
        services.AddSingleton<Application.Services.IImportQueueService>(sp => sp.GetRequiredService<Infrastructure.Services.BackgroundImportQueueService>());
        services.AddHostedService(sp => sp.GetRequiredService<Infrastructure.Services.BackgroundImportQueueService>());

        // Processing services
        services.AddScoped<Application.Services.IProcessingService, Infrastructure.Services.ProcessingService>();
        services.AddScoped<Application.Services.IRowProcessor, Infrastructure.Services.Processors.ComputeStatisticsProcessor>();

        // ✅ NEW: CRM Service
        services.AddScoped<Application.Services.ICrmService, Infrastructure.Services.CrmService>();

        // Cleanup hosted service
        services.AddSingleton<Infrastructure.Services.CleanupHostedService>();
        services.AddHostedService(sp => sp.GetRequiredService<Infrastructure.Services.CleanupHostedService>());

        return services;
    }
}