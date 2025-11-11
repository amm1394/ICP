using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Core.Icp.Domain.Entities.Elements;

namespace Infrastructure.Icp.Data.Configurations
{
    public class CalibrationCurveConfiguration : IEntityTypeConfiguration<CalibrationCurve>
    {
        public void Configure(EntityTypeBuilder<CalibrationCurve> builder)
        {
            builder.ToTable("CalibrationCurves");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Notes)
                .HasMaxLength(1000);

            builder.Property(c => c.CreatedBy)
                .HasMaxLength(100);

            builder.Property(c => c.UpdatedBy)
                .HasMaxLength(100);

            // Index ها
            builder.HasIndex(c => c.ElementId)
                .HasDatabaseName("IX_CalibrationCurve_ElementId");

            builder.HasIndex(c => c.ProjectId)
                .HasDatabaseName("IX_CalibrationCurve_ProjectId");

            builder.HasIndex(c => c.CalibrationDate)
                .HasDatabaseName("IX_CalibrationCurve_Date");

            // روابط
            builder.HasOne(c => c.Element)
                .WithMany()
                .HasForeignKey(c => c.ElementId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.Project)
                .WithMany(p => p.CalibrationCurves)
                .HasForeignKey(c => c.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(c => c.Points)
                .WithOne(p => p.CalibrationCurve)
                .HasForeignKey(p => p.CalibrationCurveId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}