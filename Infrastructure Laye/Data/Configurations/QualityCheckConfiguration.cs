using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Core.Icp.Domain.Entities.QualityControl;

namespace Infrastructure.Icp.Data.Configurations
{
    public class QualityCheckConfiguration : IEntityTypeConfiguration<QualityCheck>
    {
        public void Configure(EntityTypeBuilder<QualityCheck> builder)
        {
            builder.ToTable("QualityChecks");

            builder.HasKey(q => q.Id);

            builder.Property(q => q.Message)
                .HasMaxLength(500);

            builder.Property(q => q.CreatedBy)
                .HasMaxLength(100);

            builder.Property(q => q.UpdatedBy)
                .HasMaxLength(100);

            // Index ها
            builder.HasIndex(q => q.SampleId)
                .HasDatabaseName("IX_QualityCheck_SampleId");

            builder.HasIndex(q => q.CheckType)
                .HasDatabaseName("IX_QualityCheck_Type");

            builder.HasIndex(q => q.Status)
                .HasDatabaseName("IX_QualityCheck_Status");

            builder.HasIndex(q => q.CheckDate)
                .HasDatabaseName("IX_QualityCheck_Date");

            // روابط
            builder.HasOne(q => q.Sample)
                .WithMany(s => s.QualityChecks)
                .HasForeignKey(q => q.SampleId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}