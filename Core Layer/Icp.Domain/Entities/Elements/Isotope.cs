using Core.Icp.Domain.Base;

namespace Core.Icp.Domain.Entities.Elements
{
    /// <summary>
    /// Represents an isotope of a chemical element.
    /// </summary>
    /// <remarks>
    /// An isotope shares the same number of protons (atomic number) as its element but has a different
    /// number of neutrons, resulting in a different mass number.
    /// </remarks>
    public class Isotope : BaseEntity
    {
        /// <summary>
        /// Gets or sets the foreign key for the element this isotope belongs to.
        /// </summary>
        public Guid ElementId { get; set; }

        /// <summary>
        /// Gets or sets the mass number of the isotope (total number of protons and neutrons).
        /// </summary>
        public int MassNumber { get; set; }

        /// <summary>
        /// Gets or sets the natural abundance of the isotope, typically expressed as a percentage (0-100).
        /// </summary>
        public decimal Abundance { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the isotope is stable (i.e., non-radioactive).
        /// </summary>
        public bool IsStable { get; set; }

        // Navigation Properties
        /// <summary>
        /// Gets or sets the navigation property to the parent <see cref="Element"/> entity.
        /// </summary>
        public Element Element { get; set; } = null!;
    }
}