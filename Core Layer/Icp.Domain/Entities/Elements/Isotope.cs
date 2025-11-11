using Core.Icp.Domain.Base;

namespace Core.Icp.Domain.Entities.Elements
{
    /// <summary>
    /// ایزوتوپ یک عنصر
    /// </summary>
    public class Isotope : BaseEntity
    {
        public Guid ElementId { get; set; }

        public int MassNumber { get; set; }
        public decimal Abundance { get; set; }
        public bool IsStable { get; set; }

        // Navigation Properties
        public Element Element { get; set; } = null!;
    }
}