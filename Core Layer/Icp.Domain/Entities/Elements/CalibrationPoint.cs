using Core.Icp.Domain.Base;

namespace Core.Icp.Domain.Entities.Elements
{
    /// <summary>
    /// یک نقطه در منحنی کالیبراسیون
    /// </summary>
    public class CalibrationPoint : BaseEntity
    {
        public Guid CalibrationCurveId { get; set; }

        public decimal Concentration { get; set; }
        public decimal Intensity { get; set; }
        public int PointOrder { get; set; }

        // Navigation Properties
        public CalibrationCurve CalibrationCurve { get; set; } = null!;
    }
}