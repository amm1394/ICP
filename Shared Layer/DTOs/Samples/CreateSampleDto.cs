namespace Shared.Icp.DTOs.Samples
{
    /// <summary>
    /// DTO برای ایجاد نمونه جدید
    /// </summary>
    public class CreateSampleDto
    {
        /// <summary>
        /// شناسه نمونه (Sample ID از آزمایشگاه)
        /// </summary>
        public string SampleId { get; set; } = string.Empty;

        /// <summary>
        /// نام نمونه
        /// </summary>
        public string SampleName { get; set; } = string.Empty;

        /// <summary>
        /// تاریخ اجرای تست (اختیاری - اگر نباشد، تاریخ فعلی استفاده می‌شود)
        /// </summary>
        public DateTime? RunDate { get; set; }

        /// <summary>
        /// وزن نمونه (گرم) - اختیاری
        /// </summary>
        public decimal? Weight { get; set; }

        /// <summary>
        /// حجم نمونه (میلی‌لیتر) - اختیاری
        /// </summary>
        public decimal? Volume { get; set; }

        /// <summary>
        /// ضریب رقیق‌سازی - اختیاری (پیش‌فرض: 1)
        /// </summary>
        public decimal? DilutionFactor { get; set; }

        /// <summary>
        /// یادداشت‌ها - اختیاری
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// شناسه پروژه (الزامی)
        /// </summary>
        public Guid ProjectId { get; set; }
    }
}