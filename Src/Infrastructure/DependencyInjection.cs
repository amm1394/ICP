using Domain.Interfaces;
using Infrastructure.FileProcessing; // برای CsvFileService و ExcelService
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Reports; // برای ExcelExportService
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. تنظیمات دیتابیس
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        // 2. ثبت UnitOfWork
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // 3. ثبت سرویس‌های ایمپورت فایل (فاز ۲)
        // چون هر دو از یک اینترفیس ارث می‌برند، به صورت کالکشن ثبت می‌شوند
        services.AddScoped<IFileImportService, ExcelService>();
        services.AddScoped<IFileImportService, CsvFileService>();

        // 4. ثبت سرویس خروجی اکسل (فاز ۵)
        services.AddScoped<IExcelExportService, ExcelExportService>();

        return services;
    }
}