namespace Core.Icp.Domain.Models.Files
{
    public class FileRecordError
    {
        public int RowNumber { get; set; }          // شماره ردیف در فایل (۱-based یا ۰-based، فقط ثابت نگه‌دار)
        public string? SampleCode { get; set; }     // اگر از روی ردیف قابل تشخیص است
        public string? ColumnName { get; set; }     // ستون مشکل‌دار (اگر می‌دونی)
        public string Message { get; set; } = null!;
        public string? ErrorCode { get; set; }      // مثلاً MISSING_REQUIRED_COLUMN, INVALID_NUMBER, ...
        public bool IsWarning { get; set; }         // true=Warning, false=Error
    }
}
