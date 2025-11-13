using Core.Icp.Domain.Enums;

namespace Infrastructure.Icp.Reports.Models.Configurations
{
    /// <summary>
    /// تنظیمات فرمت‌بندی Excel
    /// </summary>
    public class ExcelFormattingOptions
    {
        /// <summary>
        /// تنظیم خودکار عرض ستون‌ها
        /// </summary>
        public bool AutoFitColumns { get; set; } = true;

        /// <summary>
        /// ثابت کردن ردیف هدر
        /// </summary>
        public bool FreezeHeader { get; set; } = true;

        /// <summary>
        /// تعداد ردیف‌های ثابت
        /// </summary>
        public int FreezeRowCount { get; set; } = 1;

        /// <summary>
        /// تعداد ستون‌های ثابت
        /// </summary>
        public int FreezeColumnCount { get; set; } = 0;

        /// <summary>
        /// اعمال استایل جدول
        /// </summary>
        public bool ApplyTableStyle { get; set; } = true;

        /// <summary>
        /// نام استایل جدول
        /// </summary>
        public string TableStyleName { get; set; } = "TableStyleMedium2";

        /// <summary>
        /// فرمت‌های ستون
        /// </summary>
        public Dictionary<string, ColumnFormat> ColumnFormats { get; set; } = new();

        /// <summary>
        /// رنگ هدر
        /// </summary>
        public string HeaderBackgroundColor { get; set; } = "#4472C4";

        /// <summary>
        /// رنگ متن هدر
        /// </summary>
        public string HeaderForegroundColor { get; set; } = "#FFFFFF";

        /// <summary>
        /// فونت هدر
        /// </summary>
        public string HeaderFontFamily { get; set; } = "B Nazanin";

        /// <summary>
        /// اندازه فونت هدر
        /// </summary>
        public int HeaderFontSize { get; set; } = 12;

        /// <summary>
        /// هدر بولد باشد
        /// </summary>
        public bool HeaderBold { get; set; } = true;

        /// <summary>
        /// فونت داده‌ها
        /// </summary>
        public string DataFontFamily { get; set; } = "B Nazanin";

        /// <summary>
        /// اندازه فونت داده‌ها
        /// </summary>
        public int DataFontSize { get; set; } = 11;

        /// <summary>
        /// رنگ ردیف‌های زوج
        /// </summary>
        public string? AlternateRowColor { get; set; } = "#F2F2F2";

        /// <summary>
        /// فیلتر خودکار
        /// </summary>
        public bool EnableAutoFilter { get; set; } = true;

        /// <summary>
        /// نمایش خطوط شبکه
        /// </summary>
        public bool ShowGridLines { get; set; } = true;

        /// <summary>
        /// محافظت از Sheet
        /// </summary>
        public bool ProtectSheet { get; set; } = false;

        /// <summary>
        /// رمز عبور محافظت (در صورت نیاز)
        /// </summary>
        public string? ProtectionPassword { get; set; }

        /// <summary>
        /// زوم پیش‌فرض (درصد)
        /// </summary>
        public int DefaultZoom { get; set; } = 100;

        /// <summary>
        /// جهت متن
        /// </summary>
        public ExcelTextDirection TextDirection { get; set; } = ExcelTextDirection.RightToLeft;
    }
}