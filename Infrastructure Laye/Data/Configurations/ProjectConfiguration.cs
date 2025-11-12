using Core.Icp.Domain.Entities.Projects;
using Core.Icp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class ProjectConfiguration : IEntityTypeConfiguration<Project>
    {
        public void Configure(EntityTypeBuilder<Project> builder)
        {
            builder.ToTable("Projects");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(p => p.Description)
                .HasMaxLength(1000);

            builder.Property(p => p.SourceFileName)
                .HasMaxLength(500);

            // تبدیل Enum به String در دیتابیس
            builder.Property(p => p.Status)
                .HasConversion<string>()  // ← این خط مهمه - enum رو به string تبدیل می‌کنه
                .HasMaxLength(50)
                .IsRequired();

            // Relationships
            builder.HasMany(p => p.Samples)
                .WithOne(s => s.Project)
                .HasForeignKey(s => s.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.CalibrationCurves)
                .WithOne(c => c.Project)
                .HasForeignKey(c => c.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(p => p.Name);
            builder.HasIndex(p => p.Status);
            builder.HasIndex(p => p.CreatedAt);
        }
    }
}