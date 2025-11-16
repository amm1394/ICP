using System;
using System.Collections.Generic;

namespace Presentation.Icp.API.Models.Projects
{
    /// <summary>
    /// آیتم ساده شده برای نمایش Sample ها در جزئیات پروژه.
    /// </summary>
    public class ProjectSampleItemDto
    {
        public Guid Id { get; set; }

        public string Code { get; set; } = string.Empty;

        public string? Description { get; set; }

        /// <summary>
        /// تعداد Measurement های این Sample.
        /// </summary>
        public int MeasurementCount { get; set; }
    }
}
