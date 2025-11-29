using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // اینجا سرویس‌هایی که در لایه Application هست (مثل validators، mappings، MediatR) را ثبت کن.
        // در حال حاضر فقط placeholder است.
        return services;
    }
}