using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Core.Icp.Domain.Entities.Elements;

namespace Infrastructure.Icp.Data.Configurations
{
    public class ElementConfiguration : IEntityTypeConfiguration<Element>
    {
        public void Configure(EntityTypeBuilder<Element> builder)
        {
            builder.ToTable("Elements");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Symbol)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.CreatedBy)
                .HasMaxLength(100);

            builder.Property(e => e.UpdatedBy)
                .HasMaxLength(100);

            // Index ها
            builder.HasIndex(e => e.Symbol)
                .IsUnique()
                .HasDatabaseName("IX_Element_Symbol");

            builder.HasIndex(e => e.AtomicNumber)
                .IsUnique()
                .HasDatabaseName("IX_Element_AtomicNumber");

            builder.HasIndex(e => e.DisplayOrder)
                .HasDatabaseName("IX_Element_DisplayOrder");

            // روابط
            builder.HasMany(e => e.Isotopes)
                .WithOne(i => i.Element)
                .HasForeignKey(i => i.ElementId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}