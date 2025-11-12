using Core.Icp.Domain.Entities.Samples;
using Core.Icp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class SampleConfiguration : IEntityTypeConfiguration<Sample>
    {
        public void Configure(EntityTypeBuilder<Sample> builder)
        {
            builder.ToTable("Samples");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.SampleId)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(s => s.SampleName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(s => s.RunDate)
                .IsRequired();

            // تبدیل Enum به String
            builder.Property(s => s.Status)
                .HasConversion<string>()  // ← این خط رو اضافه کن
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(s => s.Weight)
                .HasPrecision(18, 4)
                .IsRequired();

            builder.Property(s => s.Volume)
                .HasPrecision(18, 4)
                .IsRequired();

            builder.Property(s => s.DilutionFactor)
                .IsRequired();

            // Relationships
            builder.HasOne(s => s.Project)
                .WithMany(p => p.Samples)
                .HasForeignKey(s => s.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(s => s.Measurements)
                .WithOne(m => m.Sample)
                .HasForeignKey(m => m.SampleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(s => s.QualityChecks)
                .WithOne(q => q.Sample)
                .HasForeignKey(q => q.SampleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(s => s.SampleId).IsUnique();
            builder.HasIndex(s => s.ProjectId);
            builder.HasIndex(s => s.Status);
            builder.HasIndex(s => s.RunDate);
        }
    }
}