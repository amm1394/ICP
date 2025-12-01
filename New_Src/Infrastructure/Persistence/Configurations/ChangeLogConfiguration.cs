using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ChangeLogConfiguration : IEntityTypeConfiguration<ChangeLog>
{
    public void Configure(EntityTypeBuilder<ChangeLog> builder)
    {
        // نام جدول
        builder.ToTable("ChangeLogs");

        // کلید اصلی
        builder.HasKey(e => e.Id);

        // ایندکس‌ها برای جستجوی سریع
        builder.HasIndex(e => e.ProjectId)
            .HasDatabaseName("IX_ChangeLogs_ProjectId");

        builder.HasIndex(e => e.Timestamp)
            .HasDatabaseName("IX_ChangeLogs_Timestamp");

        builder.HasIndex(e => new { e.ProjectId, e.ChangeType })
            .HasDatabaseName("IX_ChangeLogs_ProjectId_ChangeType");

        builder.HasIndex(e => new { e.ProjectId, e.SolutionLabel })
            .HasDatabaseName("IX_ChangeLogs_ProjectId_SolutionLabel");

        builder.HasIndex(e => e.BatchId)
            .HasDatabaseName("IX_ChangeLogs_BatchId");

        // تنظیمات Property ها
        builder.Property(e => e.ChangeType)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.SolutionLabel)
            .HasMaxLength(200);

        builder.Property(e => e.Element)
            .HasMaxLength(100);

        builder.Property(e => e.OldValue)
            .HasMaxLength(4000);

        builder.Property(e => e.NewValue)
            .HasMaxLength(4000);

        builder.Property(e => e.ChangedBy)
            .HasMaxLength(200);

        builder.Property(e => e.Details)
            .HasMaxLength(4000);

        builder.Property(e => e.Timestamp)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        // رابطه با Project
        builder.HasOne(e => e.Project)
            .WithMany()
            .HasForeignKey(e => e.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}