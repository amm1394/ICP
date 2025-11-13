namespace Infrastructure.Icp.Reports.Models
{
    /// <summary>
    /// نتیجه تولید گزارش
    /// </summary>
    public class ReportResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? FilePath { get; set; }
        public byte[]? FileContent { get; set; }
        public string? FileName { get; set; }
        public long FileSize { get; set; }
        public TimeSpan GenerationTime { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public Dictionary<string, object> Metadata { get; set; } = new();

        public static ReportResult CreateSuccess(string filePath, byte[] content, string fileName)
        {
            return new ReportResult
            {
                Success = true,
                Message = "گزارش با موفقیت تولید شد",
                FilePath = filePath,
                FileContent = content,
                FileName = fileName,
                FileSize = content.Length
            };
        }

        public static ReportResult CreateSuccess(byte[] content, string fileName)
        {
            return new ReportResult
            {
                Success = true,
                Message = "گزارش با موفقیت تولید شد",
                FileContent = content,
                FileName = fileName,
                FileSize = content.Length
            };
        }

        public static ReportResult CreateFailure(string message, List<string>? errors = null)
        {
            return new ReportResult
            {
                Success = false,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }
    }
}