using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Core.Icp.Domain.Entities.CRM;

namespace Infrastructure.Icp.Data.Configurations
{
    public class CRMValueConfiguration : IEntityTypeConfiguration<CRMValue>
    {
        public void Configure(EntityTypeBuilder<CRMValue> builder)
        {
            builder.ToTable("CRMValues");

            builder.HasKey(v => v.Id);

            builder.Property(v => v.Unit)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(v => v.CreatedBy)
                .HasMaxLength(100);

            builder.Property(v => v.UpdatedBy)
                .HasMaxLength(100);

            // Index ها
            builder.HasIndex(v => v.CRMId)
                .HasDatabaseName("IX_CRMValue_CRMId");

            builder.HasIndex(v => v.ElementId)
                .HasDatabaseName("IX_CRMValue_ElementId");

            builder.HasIndex(v => new { v.CRMId, v.ElementId })
                .IsUnique()
                .HasDatabaseName("IX_CRMValue_CRM_Element");

            // روابط
            builder.HasOne(v => v.CRM)
                .WithMany(c => c.CertifiedValues)
                .HasForeignKey(v => v.CRMId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(v => v.Element)
                .WithMany()
                .HasForeignKey(v => v.ElementId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}