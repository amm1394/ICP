using Core.Icp.Domain.Entities.Projects;

namespace Core.Icp.Domain.Models.Files
{
    /// <summary>
    /// نتیجهٔ سطح دامنه برای عملیات ایمپورت فایل و ایجاد/به‌روزرسانی پروژه.
    /// این مدل داخل لایهٔ Domain استفاده می‌شود و مستقیماً در API اکسپوز نمی‌شود.
    /// </summary>
    public class ProjectImportResult
    {
        /// <summary>
        /// پروژهٔ ایجاد یا به‌روزرسانی شده.
        /// </summary>
        public Project Project { get; set; } = null!;

        /// <summary>
        /// وضعیت کلی موفقیت/شکست عملیات ایمپورت (با درنظرگرفتن خطاهای ساختاری).
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// پیام کلی عملیات ایمپورت (موفقیت/هشدار/خطای کلی).
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// تعداد کل رکوردهایی که از فایل خوانده شده‌اند.
        /// </summary>
        public int TotalRecords { get; set; }

        /// <summary>
        /// تعداد رکوردهایی که با موفقیت به Sample/Measurement تبدیل و ذخیره شده‌اند.
        /// </summary>
        public int SuccessfulRecords { get; set; }

        /// <summary>
        /// تعداد رکوردهایی که به‌دلیل خطا رد شده‌اند.
        /// </summary>
        public int FailedRecords { get; set; }

        /// <summary>
        /// تعداد نمونه‌های نهایی ذخیره‌شده در پروژه.
        /// </summary>
        public int TotalSamples { get; set; }

        /// <summary>
        /// خطاهای سطح عملیات ایمپورت (ساختار فایل، ردیف‌های مشکل‌دار و ...).
        /// </summary>
        public List<string> Errors { get; set; } = new();

        /// <summary>
        /// هشدارها (مثلاً ستون‌های اضافی، داده‌های Missing قابل چشم‌پوشی و ...).
        /// </summary>
        public List<string> Warnings { get; set; } = new();
    }
}