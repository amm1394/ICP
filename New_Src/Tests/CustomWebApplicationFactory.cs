using Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // 1) همه رجیستریشن‌های قبلی مربوط به IsatisDbContext را حذف کن
            var descriptorsToRemove = services
                .Where(d =>
                    d.ServiceType == typeof(DbContextOptions<IsatisDbContext>) ||
                    d.ServiceType == typeof(IsatisDbContext))
                .ToList();

            foreach (var descriptor in descriptorsToRemove)
            {
                services.Remove(descriptor);
            }

            // 2) اگر TEST_SQL_CONNECTION ست شده بود، از SQL Server برای تست استفاده کن
            var sqlConn = Environment.GetEnvironmentVariable("TEST_SQL_CONNECTION");
            if (!string.IsNullOrWhiteSpace(sqlConn))
            {
                services.AddDbContext<IsatisDbContext>(options =>
                {
                    options.UseSqlServer(sqlConn, sqlOptions =>
                    {
                        // در صورت نیاز برای CI/تست‌های سنگین
                        sqlOptions.CommandTimeout(180);
                    });
                });

                // ساخت provider و اعمال مایگریشن‌ها
                using var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<IsatisDbContext>();

                db.Database.Migrate();
            }
            else
            {
                // 3) حالت پیش‌فرض: استفاده از InMemory برای تست‌ها
                services.AddDbContext<IsatisDbContext>(options =>
                {
                    options.UseInMemoryDatabase("Isatis_TestDb");
                });

                // ساخت provider و پاک/ایجاد دیتابیس InMemory
                using var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<IsatisDbContext>();

                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                // اگر لازم شد، اینجا می‌توانی Seed هم انجام بدهی
                // SeedTestData.Initialize(db);
            }
        });
    }
}
