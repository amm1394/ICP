namespace Presentation.Icp.API.Models.Projects
{
    /// <summary>
    /// DTO خلاصه اطلاعات پروژه برای لیست‌ها و جزئیات ساده.
    /// </summary>
    public class ProjectSummaryDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public int SampleCount { get; set; }
    }
}
