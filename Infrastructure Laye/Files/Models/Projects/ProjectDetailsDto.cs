using System;
using System.Collections.Generic;

namespace Presentation.Icp.API.Models.Projects
{
    /// <summary>
    /// DTO جزئیات پروژه.
    /// </summary>
    public class ProjectDetailsDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; }

        public string Status { get; set; } = string.Empty;

        public int SampleCount { get; set; }

        /// <summary>
        /// لیست نمونه‌ها به صورت خلاصه (فقط برای نمایش سریع).
        /// اگر بعداً نیاز شد، می‌توان آن را گسترش داد.
        /// </summary>
        public List<ProjectSampleItemDto> Samples { get; set; } = new();
    }
}
