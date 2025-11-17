using Microsoft.EntityFrameworkCore;
using Core.Icp.Domain.Entities.Samples;
using Core.Icp.Domain.Entities.Elements;
using Core.Icp.Domain.Entities.Projects;
using Core.Icp.Domain.Entities.QualityControl;

namespace Infrastructure.Icp.Data.Context
{
    /// <summary>
    /// DbContext اصلی برای Isatis.NET
    /// </summary>
    public class ICPDbContext : DbContext
    {
        public ICPDbContext(DbContextOptions<ICPDbContext> options)
            : base(options)
        {
        }

        // DbSets برای Entity ها
        public DbSet<Sample> Samples { get; set; }
        public DbSet<Measurement> Measurements { get; set; }

        public DbSet<Element> Elements { get; set; }
        public DbSet<Isotope> Isotopes { get; set; }
        public DbSet<CalibrationCurve> CalibrationCurves { get; set; }
        public DbSet<CalibrationPoint> CalibrationPoints { get; set; }

        public DbSet<CRM> CRMs { get; set; }
        public DbSet<CRMValue> CRMValues { get; set; }

        public DbSet<Project> Projects { get; set; }

        public DbSet<QualityCheck> QualityChecks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // اعمال تمام Configurations
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ICPDbContext).Assembly);

            // تنظیمات کلی
            ConfigureConventions(modelBuilder);
        }

        private void ConfigureConventions(ModelBuilder modelBuilder)
        {
            // تنظیم Decimal ها
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(decimal) || property.ClrType == typeof(decimal?))
                    {
                        property.SetPrecision(18);
                        property.SetScale(6);
                    }
                }
            }

            // Query Filter برای Soft Delete
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(Core.Icp.Domain.Base.BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                    var propertyMethod = typeof(Microsoft.EntityFrameworkCore.EF)
                        .GetMethod(nameof(Microsoft.EntityFrameworkCore.EF.Property))
                        ?.MakeGenericMethod(typeof(bool));

                    var isDeletedProperty = System.Linq.Expressions.Expression.Call(
                        propertyMethod,
                        parameter,
                        System.Linq.Expressions.Expression.Constant("IsDeleted"));

                    var filter = System.Linq.Expressions.Expression.Lambda(
                        System.Linq.Expressions.Expression.Equal(
                            isDeletedProperty,
                            System.Linq.Expressions.Expression.Constant(false)),
                        parameter);

                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
                }
            }
        }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is Core.Icp.Domain.Base.BaseEntity &&
                           (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                var entity = (Core.Icp.Domain.Base.BaseEntity)entry.Entity;

                if (entry.State == EntityState.Added)
                {
                    entity.CreatedAt = DateTime.UtcNow;
                }

                entity.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}