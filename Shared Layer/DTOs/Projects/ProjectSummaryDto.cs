using Shared.Icp.DTOs.Common;
using Core.Icp.Domain.Enums;

namespace Shared.Icp.DTOs.Projects
{
    /// <summary>
    /// Presentation DTO providing a compact summary of a project for list/grid views.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Intended for lightweight listings and dashboards where only essential project fields are required. It inherits from
    /// <see cref="BaseDto"/>, therefore also carries common metadata such as <see cref="BaseDto.Id"/>,
    /// <see cref="BaseDto.CreatedAt"/>, and <see cref="BaseDto.UpdatedAt"/>.
    /// </para>
    /// <para>
    /// Notes:
    /// - Prefer UTC timestamps for date/time values like <see cref="StartDate"/> (ISO-8601 in JSON) to avoid time zone issues.
    /// - Status values should follow your domain conventions (e.g., Active, Archived, Completed) even though it is a string here.
    /// - User-facing messages elsewhere (validation/errors/success) remain Persian; this DTO only carries data.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var summary = new ProjectSummaryDto
    /// {
    ///     Id = Guid.NewGuid(),
    ///     CreatedAt = DateTime.UtcNow,
    ///     Name = "River Monitoring Q2",
    ///     Description = "Quarterly assessment for the northern basin.",
    ///     Status = "Active",
    ///     StartDate = new DateTime(2025, 04, 01, 00, 00, 00, DateTimeKind.Utc),
    ///     SampleCount = 156
    /// };
    /// </code>
    /// </example>
    /// <seealso cref="ProjectDto"/>
    /// <seealso cref="CreateProjectDto"/>
    /// <seealso cref="UpdateProjectDto"/>
    public class ProjectSummaryDto : BaseDto
    {
        /// <summary>
        /// Project display name.
        /// </summary>
        /// <remarks>
        /// Shown in list views and dashboards to identify the project.
        /// </remarks>
        /// <value>
        /// Non-empty string.
        /// </value>
        /// <example>
        /// River Monitoring Q2
        /// </example>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Short project description (optional).
        /// </summary>
        /// <remarks>
        /// Use for concise context such as scope, location, or objectives. May be truncated in UIs.
        /// </remarks>
        /// <value>
        /// String or null.
        /// </value>
        /// <example>
        /// Baseline survey focusing on heavy metals.
        /// </example>
        public string? Description { get; set; }

        /// <summary>
        /// Project status label (optional).
        /// </summary>
        /// <remarks>
        /// Domain-defined value, typically something like: Active, Archived, Completed.
        /// </remarks>
        /// <value>
        /// String or null.
        /// </value>
        /// <example>
        /// Active
        /// </example>
        public string? Status { get; set; }

        /// <summary>
        /// Project start date/time (optional).
        /// </summary>
        /// <remarks>
        /// Prefer UTC for storage/transfer (ISO-8601 in JSON). When null, the start date is unspecified.
        /// </remarks>
        /// <value>
        /// Date/time value or null.
        /// </value>
        /// <example>
        /// 2025-04-01T00:00:00Z
        /// </example>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Total count of samples associated with the project.
        /// </summary>
        /// <remarks>
        /// Useful for quick overviews and capacity planning. May be filtered by query context.
        /// </remarks>
        /// <value>
        /// Non-negative integer.
        /// </value>
        /// <example>
        /// 156
        /// </example>
        public int SampleCount { get; set; }
    }
}