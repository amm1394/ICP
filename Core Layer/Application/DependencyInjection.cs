using Core.Icp.Application.Services.Projects;
using Core.Icp.Domain.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Icp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // سرویس پروژه‌ها
        services.AddScoped<IProjectService, ProjectService>();

        // در فازهای بعدی:
        // services.AddScoped<ISampleService, SampleService>();
        // services.AddScoped<IQualityControlService, QualityControlService>();
        // ...

        return services;
    }
}
