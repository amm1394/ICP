using Core.Icp.Domain.Entities.Projects;

namespace Core.Icp.Domain.Models.Files
{
    /// <summary>
    /// نتیجه سطح دامنه‌ای برای ایمپورت فایل و ایجاد/به‌روزرسانی پروژه.
    /// این مدل بین Application Service و لایه Presentation رد و بدل نمی‌شود
    /// بلکه برای سرویس‌های داخلی و map شدن به DTOهای API استفاده می‌شود.
    /// </summary>
    public class ProjectImportResult
    {
        /// <summary>
        /// پروژه‌ی ایجاد/به‌روزرسانی شده.
        /// </summary>
        public Project Project { get; set; } = null!;

        /// <summary>
        /// آیا عملیات ایمپورت به‌طور کلی موفق بوده است یا خیر (با توجه به Validation فایل).
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// پیام خلاصه‌ی ایمپورت (مثلاً "فایل با موفقیت پردازش شد").
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// تعداد کل رکوردهای پردازش‌شده از فایل (مثلاً تعداد ردیف‌ها).
        /// </summary>
        public int TotalRecords { get; set; }

        /// <summary>
        /// تعداد رکوردهایی که بدون مشکل پردازش شده‌اند.
        /// </summary>
        public int SuccessfulRecords { get; set; }

        /// <summary>
        /// تعداد رکوردهایی که به دلیل خطا (Validation یا Parsing) پردازش نشده‌اند.
        /// </summary>
        public int FailedRecords { get; set; }

        /// <summary>
        /// تعداد نمونه‌ها (Samples) که در پروژه ایجاد شده‌اند.
        /// </summary>
        public int TotalSamples { get; set; }

        /// <summary>
        /// خطاهای سطح بالا (مثلاً خطای ساختاری فایل یا ردیف‌های نامعتبر).
        /// فعلاً از Validation فایل می‌آید و در آینده می‌تواند غنی‌تر شود.
        /// </summary>
        public List<string> Errors { get; set; } = new();

        /// <summary>
        /// هشدارها (مثلاً ستون‌های اضافی، مقدار Missing ولی قابل‌جبران و ...).
        /// </summary>
        public List<string> Warnings { get; set; } = new();
    }
}
