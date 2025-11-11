using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Core.Icp.Domain.Entities.Elements;

namespace Infrastructure.Icp.Data.Configurations
{
    public class IsotopeConfiguration : IEntityTypeConfiguration<Isotope>
    {
        public void Configure(EntityTypeBuilder<Isotope> builder)
        {
            builder.ToTable("Isotopes");

            builder.HasKey(i => i.Id);

            builder.Property(i => i.CreatedBy)
                .HasMaxLength(100);

            builder.Property(i => i.UpdatedBy)
                .HasMaxLength(100);

            // Index ها
            builder.HasIndex(i => i.ElementId)
                .HasDatabaseName("IX_Isotope_ElementId");

            builder.HasIndex(i => new { i.ElementId, i.MassNumber })
                .IsUnique()
                .HasDatabaseName("IX_Isotope_Element_Mass");

            // روابط
            builder.HasOne(i => i.Element)
                .WithMany(e => e.Isotopes)
                .HasForeignKey(i => i.ElementId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}