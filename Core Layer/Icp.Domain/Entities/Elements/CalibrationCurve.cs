using Core.Icp.Domain.Base;
using Core.Icp.Domain.Entities.Projects;

namespace Core.Icp.Domain.Entities.Elements
{
    /// <summary>
    /// منحنی کالیبراسیون برای یک عنصر
    /// </summary>
    public class CalibrationCurve : BaseEntity
    {
        public Guid ElementId { get; set; }
        public Guid ProjectId { get; set; }

        public DateTime CalibrationDate { get; set; }

        // معادله خط: y = Slope * x + Intercept
        public decimal Slope { get; set; }
        public decimal Intercept { get; set; }
        public decimal RSquared { get; set; }

        public string? Notes { get; set; }

        // Navigation Properties
        public Element Element { get; set; } = null!;
        public Project Project { get; set; } = null!;
        public ICollection<CalibrationPoint> Points { get; set; } = new List<CalibrationPoint>();
    }
}