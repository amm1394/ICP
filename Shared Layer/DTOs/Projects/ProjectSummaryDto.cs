using Shared.Icp.DTOs.Common;

namespace Shared.Icp.DTOs.Projects
{
    /// <summary>
    /// DTO خلاصه برای نمایش پروژه در لیست‌ها
    /// </summary>
    public class ProjectSummaryDto : BaseDto
    {
        /// <summary>
        /// نام پروژه
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// توضیحات کوتاه
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// وضعیت پروژه
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// تاریخ شروع
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// تعداد نمونه‌ها
        /// </summary>
        public int SampleCount { get; set; }
    }
}