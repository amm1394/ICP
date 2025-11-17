using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core.Icp.Infrastructure.Data.Configurations
{
    public class CRMValueConfiguration : IEntityTypeConfiguration<CRMValue>
    {
        public void Configure(EntityTypeBuilder<CRMValue> builder)
        {
            builder.ToTable("CRMValues");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.CertifiedValue)
                   .HasPrecision(18, 6);

            builder.Property(x => x.Uncertainty)
                   .HasPrecision(18, 6);

            builder.Property(x => x.MinAcceptable)
                   .HasPrecision(18, 6);

            builder.Property(x => x.MaxAcceptable)
                   .HasPrecision(18, 6);

            builder.Property(x => x.Unit)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(x => x.IsActive)
                   .HasDefaultValue(true);
        }
    }
}
