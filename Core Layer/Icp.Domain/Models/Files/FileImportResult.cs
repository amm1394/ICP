using Core.Icp.Domain.Entities.Samples;

namespace Core.Icp.Domain.Models.Files
{
    /// <summary>
    /// نتیجهٔ خام ایمپورت فایل (خروجی پردازش CSV/Excel قبل از ساخت Project).
    /// </summary>
    public class FileImportResult
    {
        /// <summary>
        /// لیست نمونه‌ها (Samples) استخراج‌شده از فایل.
        /// </summary>
        public List<Sample> Samples { get; set; } = new();

        /// <summary>
        /// در صورت نیاز، لیست Measurements خام استخراج‌شده.
        /// (در حال حاضر تمرکز اصلی روی Samples است.)
        /// </summary>
        public List<Measurement> Measurements { get; set; } = new();

        /// <summary>
        /// تعداد کل ردیف‌هایی که سعی کردیم از فایل بخوانیم.
        /// </summary>
        public int TotalRecords { get; set; }          // تعداد کل ردیف‌هایی که سعی کردیم بخوانیم

        /// <summary>
        /// تعداد ردیف‌هایی که با موفقیت به دادهٔ قابل استفاده تبدیل شده‌اند.
        /// </summary>
        public int SuccessfulRecords { get; set; }     // ردیف‌هایی که واقعاً تبدیل شدند

        /// <summary>
        /// تعداد ردیف‌هایی که به‌خاطر خطا (Validation/Parsing) رد شده‌اند.
        /// </summary>
        public int FailedRecords { get; set; }         // ردیف‌هایی که به خاطر خطا رد شدند

        /// <summary>
        /// آیا عملیات ایمپورت فایل (در سطح Validation/Parsing) موفق بوده است یا خیر.
        /// اگر خطای حیاتی در ساختار فایل باشد، این مقدار false می‌شود.
        /// </summary>
        public bool Success { get; set; } = true;

        /// <summary>
        /// پیام کلی عملیات ایمپورت فایل (برای لاگ/نمایش).
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// خطاهای ردیف به ردیف یا ساختاری فایل.
        /// </summary>
        public List<FileRecordError> Errors { get; set; } = new();

        /// <summary>
        /// هشدارها (مثلاً دادهٔ ناقص قابل چشم‌پوشی، ستون اضافی و ...).
        /// </summary>
        public List<FileRecordError> Warnings { get; set; } = new();
    }
}
