using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core.Icp.Infrastructure.Data.Configurations
{
    public class CRMConfiguration : IEntityTypeConfiguration<CRM>
    {
        public void Configure(EntityTypeBuilder<CRM> builder)
        {
            builder.ToTable("CRMs");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.CRMId)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.HasIndex(x => x.CRMId)
                   .IsUnique();

            builder.Property(x => x.Name)
                   .HasMaxLength(200);

            builder.Property(x => x.Manufacturer)
                   .HasMaxLength(200);

            builder.Property(x => x.LotNumber)
                   .HasMaxLength(100);

            builder.Property(x => x.Matrix)
                   .HasMaxLength(100);

            builder.Property(x => x.Description)
                   .HasMaxLength(1000);

            builder.Property(x => x.IsActive)
                   .HasDefaultValue(true);

            builder.Property(x => x.ExpirationDate);

            builder.HasMany(x => x.CertifiedValues)
                   .WithOne(v => v.CRM)
                   .HasForeignKey(v => v.CRMId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
