using Core.Icp.Domain.Enums;
using Shared.Icp.DTOs.Common;

namespace Shared.Icp.DTOs.Projects
{
    public class ProjectDto : BaseDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? SourceFileName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public ProjectStatus Status { get; set; }
    }
}