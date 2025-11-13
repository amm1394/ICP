namespace Infrastructure.Icp.Reports.Models
{
    /// <summary>
    /// بخش‌های مختلف گزارش
    /// </summary>
    public class ReportSection
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int Order { get; set; }
        public bool IsVisible { get; set; } = true;
        public Dictionary<string, object> Data { get; set; } = new();
    }

    /// <summary>
    /// انواع بخش‌های گزارش
    /// </summary>
    public enum ReportSectionType
    {
        Header,
        ExecutiveSummary,
        SampleDetails,
        QualityControl,
        ElementStatistics,
        CRMAnalysis,
        RMAnalysis,
        Charts,
        Conclusions,
        Footer
    }
}