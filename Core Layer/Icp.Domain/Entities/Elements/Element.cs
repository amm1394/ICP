using Core.Icp.Domain.Base;

namespace Core.Icp.Domain.Entities.Elements
{
    /// <summary>
    /// عنصر شیمیایی
    /// </summary>
    public class Element : BaseEntity
    {
        public string Symbol { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        public int AtomicNumber { get; set; }
        public decimal AtomicMass { get; set; }

        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; }

        // Navigation Properties
        public ICollection<Isotope> Isotopes { get; set; } = new List<Isotope>();
    }
}