using Shared.Icp.DTOs.Common;

namespace Shared.Icp.DTOs.Projects
{
    public class ProjectDto : BaseDto  // ✅ باید از BaseDto ارث ببره
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? SourceFileName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}