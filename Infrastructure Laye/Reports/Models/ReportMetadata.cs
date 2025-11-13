namespace Infrastructure.Icp.Reports.Models
{
    /// <summary>
    /// اطلاعات متا برای گزارش
    /// </summary>
    public class ReportMetadata
    {
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Company { get; set; } = "ICP Analysis Lab";
        public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;
        public string ProjectName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Dictionary<string, string> CustomProperties { get; set; } = new();

        public static ReportMetadata CreateDefault(string projectName)
        {
            return new ReportMetadata
            {
                Title = $"گزارش تحلیل ICP-MS - {projectName}",
                Author = "ICP Analysis System",
                ProjectName = projectName,
                GeneratedDate = DateTime.UtcNow,
                Description = "گزارش کامل تحلیل داده‌های آزمایشگاهی"
            };
        }
    }
}