using Core.Icp.Domain.Enums;

namespace Infrastructure.Icp.Reports.Models.Configurations
{

    /// <summary>
    /// فرمت ستون
    /// </summary>
    public class ColumnFormat
    {
        /// <summary>
        /// عرض ستون
        /// </summary>
        public double? Width { get; set; }

        /// <summary>
        /// فرمت عددی (مثل: "#,##0.00")
        /// </summary>
        public string? NumberFormat { get; set; }

        /// <summary>
        /// تراز افقی
        /// </summary>
        public HorizontalAlignment HorizontalAlignment { get; set; } = HorizontalAlignment.General;

        /// <summary>
        /// تراز عمودی
        /// </summary>
        public VerticalAlignment VerticalAlignment { get; set; } = VerticalAlignment.Center;

        /// <summary>
        /// بولد
        /// </summary>
        public bool Bold { get; set; } = false;

        /// <summary>
        /// ایتالیک
        /// </summary>
        public bool Italic { get; set; } = false;

        /// <summary>
        /// رنگ پس‌زمینه
        /// </summary>
        public string? BackgroundColor { get; set; }

        /// <summary>
        /// رنگ متن
        /// </summary>
        public string? ForegroundColor { get; set; }

        /// <summary>
        /// Wrap Text
        /// </summary>
        public bool WrapText { get; set; } = false;
    }
}