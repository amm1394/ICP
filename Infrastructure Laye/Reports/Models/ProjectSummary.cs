namespace Infrastructure.Icp.Reports.Models
{
    /// <summary>
    /// خلاصه پروژه
    /// </summary>
    public class ProjectSummary
    {
        public string ProjectName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; } = string.Empty;

        // Statistics
        public int TotalSamples { get; set; }
        public int ProcessedSamples { get; set; }
        public int FailedSamples { get; set; }
        public int TotalElements { get; set; }
        public int TotalMeasurements { get; set; }

        // Quality Metrics
        public decimal OverallPassRate { get; set; }
        public decimal AverageAccuracy { get; set; }
        public decimal AveragePrecision { get; set; }

        // Timeline
        public TimeSpan TotalProcessingTime { get; set; }
        public TimeSpan AverageTimePerSample { get; set; }

        // Files
        public string? SourceFileName { get; set; }
        public long SourceFileSize { get; set; }

        public Dictionary<string, object> CustomMetrics { get; set; } = new();
    }
}