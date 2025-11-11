using Shared.Icp.DTOs.Common;

namespace Shared.Icp.DTOs.Elements
{
    /// <summary>
    /// DTO برای نمایش منحنی کالیبراسیون
    /// </summary>
    public class CalibrationCurveDto : BaseDto
    {
        /// <summary>
        /// شناسه عنصر
        /// </summary>
        public Guid ElementId { get; set; }

        /// <summary>
        /// شناسه پروژه
        /// </summary>
        public Guid ProjectId { get; set; }

        /// <summary>
        /// تاریخ کالیبراسیون
        /// </summary>
        public DateTime CalibrationDate { get; set; }

        /// <summary>
        /// شیب خط (Slope)
        /// </summary>
        public decimal Slope { get; set; }

        /// <summary>
        /// عرض از مبدا (Intercept)
        /// </summary>
        public decimal Intercept { get; set; }

        /// <summary>
        /// ضریب همبستگی (R²)
        /// </summary>
        public decimal RSquared { get; set; }

        /// <summary>
        /// یادداشت‌ها
        /// </summary>
        public string? Notes { get; set; }
    }
}