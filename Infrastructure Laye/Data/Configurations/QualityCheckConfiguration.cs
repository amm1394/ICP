using Core.Icp.Domain.Entities.QualityControl;
using Core.Icp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class QualityCheckConfiguration : IEntityTypeConfiguration<QualityCheck>
    {
        public void Configure(EntityTypeBuilder<QualityCheck> builder)
        {
            builder.ToTable("QualityChecks");

            builder.HasKey(q => q.Id);

            // تبدیل Enum ها به String
            builder.Property(q => q.CheckType)
                .HasConversion<string>()  // ← این رو اضافه کن
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(q => q.Status)
                .HasConversion<string>()  // ← این رو اضافه کن
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(q => q.Message)
                .HasMaxLength(500);

            builder.Property(q => q.Details)
                .HasMaxLength(2000);

            // Relationships
            builder.HasOne(q => q.Sample)
                .WithMany(s => s.QualityChecks)
                .HasForeignKey(q => q.SampleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(q => q.SampleId);
            builder.HasIndex(q => q.CheckType);
            builder.HasIndex(q => q.Status);
        }
    }
}