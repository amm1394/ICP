using Core.Icp.Domain.Enums;

namespace Infrastructure.Icp.Reports.Models.Configurations
{
    /// <summary>
    /// فیلد مقدار Pivot
    /// </summary>
    public class PivotValueField
    {
        /// <summary>
        /// نام فیلد
        /// </summary>
        public string FieldName { get; set; } = string.Empty;

        /// <summary>
        /// تابع جمع‌آوری
        /// </summary>
        public PivotFunction Function { get; set; } = PivotFunction.Sum;

        /// <summary>
        /// نام نمایشی
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// فرمت عددی
        /// </summary>
        public string? NumberFormat { get; set; }

        /// <summary>
        /// نمایش به صورت درصد
        /// </summary>
        public bool ShowAsPercentage { get; set; } = false;
    }
}