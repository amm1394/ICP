using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Core.Icp.Domain.Entities.CRM;

namespace Infrastructure.Icp.Data.Configurations
{
    public class CRMConfiguration : IEntityTypeConfiguration<CRM>
    {
        public void Configure(EntityTypeBuilder<CRM> builder)
        {
            builder.ToTable("CRMs");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.CRMId)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(c => c.Manufacturer)
                .HasMaxLength(100);

            builder.Property(c => c.LotNumber)
                .HasMaxLength(50);

            builder.Property(c => c.Notes)
                .HasMaxLength(1000);

            builder.Property(c => c.CreatedBy)
                .HasMaxLength(100);

            builder.Property(c => c.UpdatedBy)
                .HasMaxLength(100);

            // Index ها
            builder.HasIndex(c => c.CRMId)
                .IsUnique()
                .HasDatabaseName("IX_CRM_CRMId");

            builder.HasIndex(c => c.IsActive)
                .HasDatabaseName("IX_CRM_IsActive");

            // روابط
            builder.HasMany(c => c.CertifiedValues)
                .WithOne(v => v.CRM)
                .HasForeignKey(v => v.CRMId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}