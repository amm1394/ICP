using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Core.Icp.Domain.Entities.Projects;

namespace Infrastructure.Icp.Data.Configurations
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

            builder.Property(p => p.Status)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("Active");

            builder.Property(p => p.CreatedBy)
                .HasMaxLength(100);

            builder.Property(p => p.UpdatedBy)
                .HasMaxLength(100);

            // Index ها
            builder.HasIndex(p => p.Name)
                .HasDatabaseName("IX_Project_Name");

            builder.HasIndex(p => p.Status)
                .HasDatabaseName("IX_Project_Status");

            builder.HasIndex(p => p.StartDate)
                .HasDatabaseName("IX_Project_StartDate");

            // روابط
            builder.HasMany(p => p.Samples)
                .WithOne(s => s.Project)
                .HasForeignKey(s => s.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(p => p.CalibrationCurves)
                .WithOne(c => c.Project)
                .HasForeignKey(c => c.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}