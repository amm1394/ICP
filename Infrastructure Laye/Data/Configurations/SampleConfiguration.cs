using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Core.Icp.Domain.Entities.Samples;

namespace Infrastructure.Icp.Data.Configurations
{
    public class SampleConfiguration : IEntityTypeConfiguration<Sample>
    {
        public void Configure(EntityTypeBuilder<Sample> builder)
        {
            // نام جدول
            builder.ToTable("Samples");

            // کلید اصلی
            builder.HasKey(s => s.Id);

            // ویژگی‌ها
            builder.Property(s => s.SampleId)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(s => s.SampleName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(s => s.Notes)
                .HasMaxLength(1000);

            builder.Property(s => s.CreatedBy)
                .HasMaxLength(100);

            builder.Property(s => s.UpdatedBy)
                .HasMaxLength(100);

            // Index ها
            builder.HasIndex(s => s.SampleId)
                .HasDatabaseName("IX_Sample_SampleId");

            builder.HasIndex(s => s.ProjectId)
                .HasDatabaseName("IX_Sample_ProjectId");

            builder.HasIndex(s => s.Status)
                .HasDatabaseName("IX_Sample_Status");

            builder.HasIndex(s => s.RunDate)
                .HasDatabaseName("IX_Sample_RunDate");

            // روابط
            builder.HasOne(s => s.Project)
                .WithMany(p => p.Samples)
                .HasForeignKey(s => s.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(s => s.Measurements)
                .WithOne(m => m.Sample)
                .HasForeignKey(m => m.SampleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(s => s.QualityChecks)
                .WithOne(q => q.Sample)
                .HasForeignKey(q => q.SampleId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}