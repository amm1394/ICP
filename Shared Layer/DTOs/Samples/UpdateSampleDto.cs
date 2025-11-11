namespace Shared.Icp.DTOs.Samples
{
    /// <summary>
    /// DTO برای ویرایش نمونه موجود
    /// </summary>
    public class UpdateSampleDto
    {
        /// <summary>
        /// نام نمونه
        /// </summary>
        public string SampleName { get; set; } = string.Empty;

        /// <summary>
        /// تاریخ اجرای تست - اختیاری
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
        /// ضریب رقیق‌سازی - اختیاری
        /// </summary>
        public decimal? DilutionFactor { get; set; }

        /// <summary>
        /// وضعیت نمونه - اختیاری
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// یادداشت‌ها - اختیاری
        /// </summary>
        public string? Notes { get; set; }
    }
}