using Core.Icp.Domain.Entities.Projects;
using Shared.Icp.DTOs.Projects;

namespace Shared.Icp.Helpers.Mappers
{
    /// <summary>
    /// Mapper برای تبدیل Project ↔ DTO
    /// </summary>
    public static class ProjectMapper
    {
        /// <summary>
        /// تبدیل Entity به DTO
        /// </summary>
        public static ProjectDto ToDto(this Project project)
        {
            if (project == null) throw new ArgumentNullException(nameof(project));

            return new ProjectDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                SourceFileName = project.SourceFileName,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                Status = project.Status,
                CreatedAt = project.CreatedAt,
                UpdatedAt = project.UpdatedAt
            };
        }

        /// <summary>
        /// تبدیل Entity به ProjectSummaryDto
        /// </summary>
        public static ProjectSummaryDto ToSummaryDto(this Project project)
        {
            if (project == null) throw new ArgumentNullException(nameof(project));

            return new ProjectSummaryDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                Status = project.Status,
                StartDate = project.StartDate,
                SampleCount = project.Samples?.Count ?? 0,
                CreatedAt = project.CreatedAt,
                UpdatedAt = project.UpdatedAt
            };
        }

        /// <summary>
        /// تبدیل DTO به Entity (برای Create)
        /// </summary>
        public static Project ToEntity(this CreateProjectDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            return new Project
            {
                Name = dto.Name,
                Description = dto.Description,
                SourceFileName = dto.SourceFileName,
                StartDate = dto.StartDate.HasValue ? dto.StartDate.Value : DateTime.UtcNow,
                Status =  "Active"
            };
        }

        /// <summary>
        /// به‌روزرسانی Entity از DTO
        /// </summary>
        public static void UpdateFromDto(this Project project, UpdateProjectDto dto)
        {
            if (project == null) throw new ArgumentNullException(nameof(project));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            if (!string.IsNullOrWhiteSpace(dto.Name))
                project.Name = dto.Name;

            project.Description = dto.Description;

            if (dto.EndDate.HasValue)
                project.EndDate = dto.EndDate.Value;

            //if (!string.IsNullOrWhiteSpace(dto.Status))
                project.Status = dto.Status;
        }

        /// <summary>
        /// تبدیل لیست Entity به لیست DTO
        /// </summary>
        public static List<ProjectDto> ToDtoList(this IEnumerable<Project> projects)
        {
            return projects?.Select(p => p.ToDto()).ToList() ?? new List<ProjectDto>();
        }

        /// <summary>
        /// تبدیل لیست Entity به لیست Summary DTO
        /// </summary>
        public static List<ProjectSummaryDto> ToSummaryDtoList(this IEnumerable<Project> projects)
        {
            return projects?.Select(p => p.ToSummaryDto()).ToList() ?? new List<ProjectSummaryDto>();
        }
    }
}