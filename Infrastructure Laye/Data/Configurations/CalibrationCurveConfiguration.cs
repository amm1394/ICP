using Core.Icp.Domain.Entities.Elements;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core.Icp.Infrastructure.Data.Configurations
{
    public class CalibrationCurveConfiguration : IEntityTypeConfiguration<CalibrationCurve>
    {
        public void Configure(EntityTypeBuilder<CalibrationCurve> builder)
        {
            builder.ToTable("CalibrationCurves");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.FitType)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(x => x.Degree)
                   .HasDefaultValue(1);

            builder.Property(x => x.Slope)
                   .HasPrecision(18, 10);

            builder.Property(x => x.Intercept)
                   .HasPrecision(18, 10);

            builder.Property(x => x.RSquared)
                   .HasPrecision(18, 10);

            builder.Property(x => x.SettingsJson)
                   .HasMaxLength(4000);

            builder.Property(x => x.IsActive)
                   .HasDefaultValue(true);

            builder.HasOne(x => x.Element)
                   .WithMany()
                   .HasForeignKey(x => x.ElementId)
                   .OnDelete(DeleteBehavior.Cascade);


            builder.HasOne(x => x.Project)
                   .WithMany(p => p.CalibrationCurves)
                   .HasForeignKey(x => x.ProjectId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Points)
                   .WithOne(p => p.CalibrationCurve)
                   .HasForeignKey(p => p.CalibrationCurveId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
