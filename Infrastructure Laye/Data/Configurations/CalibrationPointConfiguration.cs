using Core.Icp.Domain.Entities.Elements;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core.Icp.Infrastructure.Data.Configurations
{
    public class CalibrationPointConfiguration : IEntityTypeConfiguration<CalibrationPoint>
    {
        public void Configure(EntityTypeBuilder<CalibrationPoint> builder)
        {
            builder.ToTable("CalibrationPoints");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Concentration)
                   .HasPrecision(18, 10);

            builder.Property(x => x.Intensity)
                   .HasPrecision(18, 10);

            builder.Property(x => x.Label)
                   .HasMaxLength(200);

            builder.Property(x => x.PointType)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(x => x.IsUsedInFit)
                   .HasDefaultValue(true);

            builder.Property(x => x.Order)
                   .HasDefaultValue(0);
        }
    }
}
