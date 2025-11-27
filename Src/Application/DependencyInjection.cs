using Application.Services.Calibration;
using Application.Services.CRM;
using Application.Services.QualityControl;
using Domain.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        services.AddScoped<IQualityControlService, QualityControlService>();
        services.AddScoped<ICalibrationService, CalibrationService>();
        services.AddScoped<ICrmService, CrmService>();

        var strategyType = typeof(IQualityCheckStrategy);
        var strategies = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => strategyType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        foreach (var strategy in strategies)
        {
            services.AddScoped(typeof(IQualityCheckStrategy), strategy);
        }

        return services;
    }
}