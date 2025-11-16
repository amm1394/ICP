using System;

namespace Core.Icp.Domain.Models.QualityControl
{
    /// <summary>
    /// خلاصه QC برای یک پروژه (تعداد نمونه‌ها، تعداد چک‌ها و وضعیت کلی).
    /// </summary>
    public class ProjectQualitySummary
    {
        public Guid ProjectId { get; set; }

        public int TotalSamples { get; set; }

        public int TotalChecks { get; set; }

        public int PassedCount { get; set; }

        public int WarningCount { get; set; }

        public int FailedCount { get; set; }

        /// <summary>
        /// تعداد چک‌هایی که هنوز منطق آن‌ها پیاده‌سازی نشده و به‌صورت NotImplemented برگشته‌اند.
        /// </summary>
        public int NotImplementedCount { get; set; }
    }
}
