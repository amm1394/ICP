using Application.Features.Reports; // برای ReportService
using Application.Services.Calibration;
using Application.Services.Crm;
using Application.Services.QualityControl;
using Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // ثبت MediatR برای هندلرها و کامندها
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        // ثبت سرویس‌های اصلی لایه Application
        services.AddScoped<IQualityControlService, QualityControlService>();
        services.AddScoped<ICalibrationService, CalibrationService>();
        services.AddScoped<ICrmService, CrmService>();

        // ✅ اضافه شد: سرویس گزارش‌دهی (فاز ۵)
        services.AddScoped<IReportService, ReportService>();

        // ثبت اتوماتیک تمام استراتژی‌های QC (Weight, Volume, CRM, ...)
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