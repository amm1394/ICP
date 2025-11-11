using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Core.Icp.Domain.Entities.Samples;

namespace Infrastructure.Icp.Data.Configurations
{
    public class MeasurementConfiguration : IEntityTypeConfiguration<Measurement>
    {
        public void Configure(EntityTypeBuilder<Measurement> builder)
        {
            builder.ToTable("Measurements");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.Notes)
                .HasMaxLength(1000);

            builder.Property(m => m.CreatedBy)
                .HasMaxLength(100);

            builder.Property(m => m.UpdatedBy)
                .HasMaxLength(100);

            // Index ها
            builder.HasIndex(m => m.SampleId)
                .HasDatabaseName("IX_Measurement_SampleId");

            builder.HasIndex(m => m.ElementId)
                .HasDatabaseName("IX_Measurement_ElementId");

            builder.HasIndex(m => new { m.SampleId, m.ElementId })
                .HasDatabaseName("IX_Measurement_Sample_Element");

            // روابط
            builder.HasOne(m => m.Sample)
                .WithMany(s => s.Measurements)
                .HasForeignKey(m => m.SampleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(m => m.Element)
                .WithMany()
                .HasForeignKey(m => m.ElementId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}