using Core.Icp.Domain.Base;
using Core.Icp.Domain.Entities.Elements;

namespace Core.Icp.Domain.Entities.Samples
{
    /// <summary>
    /// اندازه‌گیری یک عنصر در یک نمونه
    /// </summary>
    public class Measurement : BaseEntity
    {
        public Guid SampleId { get; set; }
        public Guid ElementId { get; set; }

        public int Isotope { get; set; }
        public decimal NetIntensity { get; set; }
        public decimal Concentration { get; set; }
        public decimal? FinalConcentration { get; set; }

        public bool IsValid { get; set; } = true;
        public string? Notes { get; set; }

        // Navigation Properties
        public Sample Sample { get; set; } = null!;
        public Element Element { get; set; } = null!;
    }
}