using Shared.Icp.DTOs.Common;
using Shared.Icp.DTOs.Elements;

namespace Shared.Icp.DTOs.Elements
{
    /// <summary>
    /// DTO برای نمایش نقطه کالیبراسیون
    /// </summary>
    public class CalibrationPointDto : BaseDto
    {
        /// <summary>
        /// شناسه منحنی کالیبراسیون
        /// </summary>
        public Guid CalibrationCurveId { get; set; }

        /// <summary>
        /// غلظت
        /// </summary>
        public decimal Concentration { get; set; }

        /// <summary>
        /// شدت سیگنال
        /// </summary>
        public decimal Intensity { get; set; }

        /// <summary>
        /// ترتیب نقطه
        /// </summary>
        public int PointOrder { get; set; }
    }
}