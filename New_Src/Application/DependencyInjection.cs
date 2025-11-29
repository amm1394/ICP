using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // ثبت سرویس‌های لایه برنامه (validators، mappers، handlers و غیره)
        // DO NOT register IProjectPersistenceService here if you want EF persistence.
        return services;
    }
}