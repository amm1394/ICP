using System.ComponentModel.DataAnnotations;
using Core.Icp.Domain.Enums;

namespace Shared.Icp.DTOs.Projects
{
    /// <summary>
    /// DTO used to update an existing project.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Intended for PUT/PATCH operations. Carries only the fields that are allowed to change on a project.
    /// Validation attributes provide basic constraints; additional domain validation should be enforced by the application layer.
    /// User-facing validation/error messages elsewhere should remain in Persian for presentation.
    /// </para>
    /// <para>
    /// Notes:
    /// - Use UTC timestamps for <see cref="EndDate"/> (ISO-8601 in JSON) to avoid time zone inconsistencies.
    /// - Status values should follow your domain conventions (e.g., Active, Archived, Completed) even though this property is a string.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new UpdateProjectDto
    /// {
    ///     Id = 123,
    ///     Name = "River Monitoring Q2",
    ///     Description = "Quarterly assessment for the northern basin.",
    ///     SourceFileName = "import_q2_2025.xlsx",
    ///     EndDate = new DateTime(2025, 06, 30, 23, 59, 59, DateTimeKind.Utc),
    ///     Status = "Active"
    /// };
    /// </code>
    /// </example>
    /// <seealso cref="CreateProjectDto"/>
    /// <seealso cref="ProjectDto"/>
    /// <seealso cref="ProjectSummaryDto"/>
    public class UpdateProjectDto
    {
        /// <summary>
        /// Unique numeric identifier of the project to update.
        /// </summary>
        /// <remarks>
        /// Required. Must correspond to an existing project in the data store.
        /// </remarks>
        /// <value>
        /// Positive integer.
        /// </value>
        /// <example>
        /// 123
        /// </example>
        [Required(ErrorMessage = "شناسه الزامی است")]
        public int Id { get; set; }

        /// <summary>
        /// Project display name.
        /// </summary>
        /// <remarks>
        /// Required. Maximum length of 200 characters.
        /// </remarks>
        /// <value>
        /// Non-empty string up to 200 characters.
        /// </value>
        /// <example>
        /// River Monitoring Q2
        /// </example>
        [Required(ErrorMessage = "نام پروژه الزامی است")]
        [StringLength(200, ErrorMessage = "نام پروژه نباید بیشتر از 200 کاراکتر باشد")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Optional project description.
        /// </summary>
        /// <remarks>
        /// Maximum length of 1000 characters. Use for context, scope, or notes.
        /// </remarks>
        /// <value>
        /// String or null.
        /// </value>
        /// <example>
        /// Baseline survey focusing on heavy metals.
        /// </example>
        [StringLength(1000, ErrorMessage = "توضیحات نباید بیشتر از 1000 کاراکتر باشد")]
        public string? Description { get; set; }

        /// <summary>
        /// Optional original source file name associated with the project.
        /// </summary>
        /// <remarks>
        /// Maximum length of 500 characters. This is metadata and does not store the file itself.
        /// </remarks>
        /// <value>
        /// String or null.
        /// </value>
        /// <example>
        /// import_q2_2025.xlsx
        /// </example>
        [StringLength(500, ErrorMessage = "نام فایل نباید بیشتر از 500 کاراکتر باشد")]
        public string? SourceFileName { get; set; }

        /// <summary>
        /// Optional project end date/time.
        /// </summary>
        /// <remarks>
        /// Prefer UTC for storage/transfer (ISO-8601 in JSON). When null, the project has no defined end.
        /// </remarks>
        /// <value>
        /// Date/time value or null.
        /// </value>
        /// <example>
        /// 2025-06-30T23:59:59Z
        /// </example>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Project status label.
        /// </summary>
        /// <remarks>
        /// Required. Domain-defined value, typically something like: Active, Archived, Completed. Maximum length of 50 characters.
        /// </remarks>
        /// <value>
        /// Non-empty string up to 50 characters.
        /// </value>
        /// <example>
        /// Active
        /// </example>
        [Required(ErrorMessage = "وضعیت الزامی است")]
        [StringLength(50, ErrorMessage = "وضعیت نباید بیشتر از 50 کاراکتر باشد")]
        public string? Status { get; set; }
    }
}