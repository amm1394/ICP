using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Core.Icp.Domain.Entities.Elements;

namespace Infrastructure.Icp.Data.Configurations
{
    public class CalibrationPointConfiguration : IEntityTypeConfiguration<CalibrationPoint>
    {
        public void Configure(EntityTypeBuilder<CalibrationPoint> builder)
        {
            builder.ToTable("CalibrationPoints");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.CreatedBy)
                .HasMaxLength(100);

            builder.Property(p => p.UpdatedBy)
                .HasMaxLength(100);

            // Index ها
            builder.HasIndex(p => p.CalibrationCurveId)
                .HasDatabaseName("IX_CalibrationPoint_CurveId");

            builder.HasIndex(p => new { p.CalibrationCurveId, p.PointOrder })
                .HasDatabaseName("IX_CalibrationPoint_Curve_Order");

            // روابط
            builder.HasOne(p => p.CalibrationCurve)
                .WithMany(c => c.Points)
                .HasForeignKey(p => p.CalibrationCurveId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}