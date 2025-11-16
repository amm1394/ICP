using System;

namespace Shared.Icp.DTOs.QualityControl
{
    /// <summary>
    /// DTO خلاصه نتایج QC برای یک پروژه.
    /// </summary>
    public class ProjectQualitySummaryDto
    {
        public Guid ProjectId { get; set; }

        public int TotalSamples { get; set; }

        public int TotalChecks { get; set; }

        public int PassedCount { get; set; }

        public int WarningCount { get; set; }

        public int FailedCount { get; set; }

        public int NotImplementedCount { get; set; }
    }
}
