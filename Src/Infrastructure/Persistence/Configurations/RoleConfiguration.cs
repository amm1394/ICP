using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        // تنظیم کلید اصلی
        builder.HasKey(r => r.Id);

        // نام نقش سیستمی (مثل Admin) باید یکتا و اجباری باشد
        builder.Property(r => r.Name)
            .HasMaxLength(50)
            .IsRequired();

        // نام نمایشی (فارسی)
        builder.Property(r => r.DisplayName)
            .HasMaxLength(100);

        builder.Property(r => r.Description)
            .HasMaxLength(250);

        // جلوگیری از تکرار نام نقش
        builder.HasIndex(r => r.Name).IsUnique();
    }
}