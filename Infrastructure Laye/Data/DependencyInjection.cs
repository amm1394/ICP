using Core.Icp.Domain.Interfaces.Repositories;
using Infrastructure.Icp.Data.Context;
using Infrastructure.Icp.Data.Repositories;
using Infrastructure.Icp.Data.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Data
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureData(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Database Context
            services.AddDbContext<ICPDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly("API")));

            // Repositories
            services.AddScoped<ISampleRepository, SampleRepository>();
            services.AddScoped<IElementRepository, ElementRepository>();
            services.AddScoped<ICRMRepository, CRMRepository>();
            services.AddScoped<IProjectRepository, ProjectRepository>();
            services.AddScoped<IQualityCheckRepository, QualityCheckRepository>();

            // Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}