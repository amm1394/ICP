namespace Infrastructure.Icp.Reports.Models.Configurations
{
    /// <summary>
    /// تنظیمات Pivot Table
    /// </summary>
    public class PivotTableConfiguration
    {
        /// <summary>
        /// نام Pivot Table
        /// </summary>
        public string PivotTableName { get; set; } = "PivotTable1";

        /// <summary>
        /// فیلدهای ردیف
        /// </summary>
        public List<string> RowFields { get; set; } = new();

        /// <summary>
        /// فیلدهای ستون
        /// </summary>
        public List<string> ColumnFields { get; set; } = new();

        /// <summary>
        /// فیلدهای مقدار
        /// </summary>
        public List<PivotValueField> ValueFields { get; set; } = new();

        /// <summary>
        /// فیلدهای فیلتر
        /// </summary>
        public List<string> FilterFields { get; set; } = new();

        /// <summary>
        /// نام Sheet مقصد
        /// </summary>
        public string TargetSheetName { get; set; } = "PivotTable";

        /// <summary>
        /// سلول شروع Pivot
        /// </summary>
        public string StartCell { get; set; } = "A1";

        /// <summary>
        /// نمایش جمع کل ردیف‌ها
        /// </summary>
        public bool ShowRowGrandTotals { get; set; } = true;

        /// <summary>
        /// نمایش جمع کل ستون‌ها
        /// </summary>
        public bool ShowColumnGrandTotals { get; set; } = true;

        /// <summary>
        /// استایل Pivot
        /// </summary>
        public string PivotStyleName { get; set; } = "PivotStyleMedium2";

        /// <summary>
        /// فیلدهای گروه‌بندی شده
        /// </summary>
        public Dictionary<string, GroupConfiguration> GroupedFields { get; set; } = new();
    }
}