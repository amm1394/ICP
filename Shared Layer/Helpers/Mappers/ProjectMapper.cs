using Core.Icp.Domain.Entities.Projects;
using Core.Icp.Domain.Enums;
using Shared.Icp.DTOs.Projects;
using Shared.Icp.Helpers.Extensions;

namespace Shared.Icp.Helpers.Mappers
{
    /// <summary>
    /// Provides mapping methods to convert between Project domain entities and their corresponding DTOs.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This static, stateless mapper centralizes projection logic so controllers/services remain focused on orchestration.
    /// It does not perform validation or persistence; it only translates between domain entities and transport models.
    /// </para>
    /// <para>
    /// Conventions:
    /// - Enum properties (e.g., <see cref="Project.Status"/>) are exposed as strings in DTOs for client friendliness.
    /// - Date/time values in DTOs are expected to be UTC (ISO-8601 in JSON) when transferred over the wire.
    /// - Null inputs to mapping methods raise <see cref="ArgumentNullException"/> to avoid silent failures.
    /// </para>
    /// </remarks>
    public static class ProjectMapper
    {
        /// <summary>
        /// Maps a <see cref="Project"/> entity to a <see cref="ProjectDto"/>.
        /// </summary>
        /// <param name="project">The project entity to map.</param>
        /// <returns>A new <see cref="ProjectDto"/> instance reflecting the entity state.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="project"/> is null.</exception>
        /// <remarks>
        /// - <see cref="Project.Status"/> is converted to its string representation.
        /// - The <c>SampleCount</c> is derived from <see cref="Project.Samples"/> (0 when the collection is null).
        /// - Timestamps are copied as-is; callers should ensure they represent UTC for external transfer.
        /// </remarks>
        public static ProjectDto ToDto(this Project project)
        {
            if (project == null) throw new ArgumentNullException(nameof(project));

            return new ProjectDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                SourceFileName = project.SourceFileName,
                Status = project.Status.ToString(),
                CreatedAt = project.CreatedAt,
                UpdatedAt = project.UpdatedAt,
                SampleCount = project.Samples?.Count ?? 0
            };
        }

        /// <summary>
        /// Maps a <see cref="Project"/> entity to a <see cref="ProjectSummaryDto"/>.
        /// </summary>
        /// <param name="project">The project entity to map.</param>
        /// <returns>A new <see cref="ProjectSummaryDto"/> instance with a compact set of fields.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="project"/> is null.</exception>
        /// <remarks>
        /// Produces a lightweight representation suitable for list/grid views. Includes only essential fields and a derived
        /// <c>SampleCount</c> (0 when the collection is null). Status is exposed as string.
        /// </remarks>
        public static ProjectSummaryDto ToSummaryDto(this Project project)
        {
            if (project == null) throw new ArgumentNullException(nameof(project));

            return new ProjectSummaryDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                Status = project.Status.ToString(),
                SampleCount = project.Samples?.Count ?? 0,
                CreatedAt = project.CreatedAt,
                UpdatedAt = project.UpdatedAt
            };
        }

        /// <summary>
        /// Maps a <see cref="CreateProjectDto"/> to a new <see cref="Project"/> entity.
        /// </summary>
        /// <param name="dto">The DTO to map from.</param>
        /// <returns>A new <see cref="Project"/> entity initialized from the DTO.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="dto"/> is null.</exception>
        /// <remarks>
        /// Defaulting rule: <see cref="Project.Status"/> is set to <see cref="ProjectStatus.Created"/> for new projects.
        /// Other values are copied as-is from the DTO; any additional defaults are handled by the domain layer.
        /// </remarks>
        public static Project ToEntity(this CreateProjectDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            return new Project
            {
                Name = dto.Name,
                Description = dto.Description,
                SourceFileName = dto.SourceFileName,
                Status = ProjectStatus.Created // Default status for new projects
            };
        }

        /// <summary>
        /// Updates an existing <see cref="Project"/> entity from an <see cref="UpdateProjectDto"/>.
        /// </summary>
        /// <param name="project">The project entity to update.</param>
        /// <param name="dto">The DTO containing the updated values.</param>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="project"/> or <paramref name="dto"/> is null.</exception>
        /// <remarks>
        /// Update rules:
        /// - <c>Name</c> is applied only when provided and non-whitespace.
        /// - <c>Description</c> and <c>SourceFileName</c> are assigned directly and may be null to clear values.
        /// - <c>Status</c> (string) is parsed case-insensitively into <see cref="ProjectStatus"/>; invalid values are ignored.
        /// - <see cref="Project.UpdatedAt"/> is set to <see cref="DateTime.UtcNow"/> as a side effect of any update.
        /// </remarks>
        public static void UpdateFromDto(this Project project, UpdateProjectDto dto)
        {
            if (project == null) throw new ArgumentNullException(nameof(project));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            // Update main fields
            if (!string.IsNullOrWhiteSpace(dto.Name))
                project.Name = dto.Name;

            project.Description = dto.Description;
            project.SourceFileName = dto.SourceFileName;

            // Convert status from string to enum (ignore invalid input)
            if (!string.IsNullOrWhiteSpace(dto.Status))
            {
                if (Enum.TryParse<ProjectStatus>(dto.Status, ignoreCase: true, out var status))
                {
                    project.Status = status;
                }
            }

            // Update timestamp to reflect modification time (UTC)
            project.UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Maps a collection of <see cref="Project"/> entities to a list of <see cref="ProjectDto"/>s.
        /// </summary>
        /// <param name="projects">The collection of project entities. When null, an empty list is returned.</param>
        /// <returns>A list of <see cref="ProjectDto"/>s in the same order as the source enumeration when applicable.</returns>
        public static List<ProjectDto> ToDtoList(this IEnumerable<Project> projects)
        {
            return projects?.Select(p => p.ToDto()).ToList() ?? new List<ProjectDto>();
        }

        /// <summary>
        /// Maps a collection of <see cref="Project"/> entities to a list of <see cref="ProjectSummaryDto"/>s.
        /// </summary>
        /// <param name="projects">The collection of project entities. When null, an empty list is returned.</param>
        /// <returns>A list of <see cref="ProjectSummaryDto"/>s in the same order as the source enumeration when applicable.</returns>
        public static List<ProjectSummaryDto> ToSummaryDtoList(this IEnumerable<Project> projects)
        {
            return projects?.Select(p => p.ToSummaryDto()).ToList() ?? new List<ProjectSummaryDto>();
        }
    }
}