using System;

namespace Core.Icp.Domain.Entities.Calibration
{
    public class CalibrationPoint // : BaseEntity
    {
        public Guid Id { get; set; }

        /// <summary>
        /// ارجاع به منحنی کالیبراسیون
        /// </summary>
        public Guid CalibrationCurveId { get; set; }
        public virtual CalibrationCurve CalibrationCurve { get; set; } = default!;

        /// <summary>
        /// غلظت CRM / استاندارد (محور X)
        /// </summary>
        public decimal Concentration { get; set; }

        /// <summary>
        /// شدت اندازه‌گیری‌شده (Intensity) (محور Y)
        /// </summary>
        public decimal Intensity { get; set; }

        /// <summary>
        /// آیا این نقطه در برازش منحنی استفاده شده است؟
        /// (برای مدیریت Outlierها یا نقاط حذف شده)
        /// </summary>
        public bool IsUsedInFit { get; set; } = true;

        /// <summary>
        /// ترتیب نقطه (برای نمایش / کالیبراسیون Segmentی)
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// لیبل Solution/Standard (مثلاً "CRM 258 Level 1")
        /// </summary>
        public string? Label { get; set; }

        /// <summary>
        /// نوع نقطه (CRM, RM, Blank, Standard, ...)
        /// </summary>
        public string PointType { get; set; } = "CRM";
    }
}
