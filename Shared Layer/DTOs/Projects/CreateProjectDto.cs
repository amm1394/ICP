using System.ComponentModel.DataAnnotations;

namespace Shared.Icp.DTOs.Projects
{
    /// <summary>
    /// DTO used to create a new project.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Intended for POST operations in the API layer. It carries the input data required to create a project and
    /// is validated via data annotations and additional domain rules as needed. User-facing validation/error
    /// messages elsewhere should remain in Persian for presentation; this DTO only transports data.
    /// </para>
    /// <para>
    /// Notes:
    /// - <see cref="Name"/> is required and limited to 200 characters.
    /// - <see cref="Description"/> is optional and limited to 1000 characters.
    /// - <see cref="SourceFileName"/> is optional and limited to 500 characters.
    /// - <see cref="StartDate"/> defaults to <see cref="System.DateTime.UtcNow"/>; prefer UTC/ISO-8601 in JSON to avoid time zone issues.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new CreateProjectDto
    /// {
    ///     Name = "River Monitoring Q2",
    ///     Description = "Quarterly assessment for the northern basin.",
    ///     SourceFileName = "import_q2_2025.xlsx",
    ///     StartDate = new DateTime(2025, 04, 01, 00, 00, 00, DateTimeKind.Utc)
    /// };
    /// </code>
    /// </example>
    /// <seealso cref="ProjectDto"/>
    /// <seealso cref="UpdateProjectDto"/>
    /// <seealso cref="ProjectSummaryDto"/>
    public class CreateProjectDto
    {
        /// <summary>
        /// Project display name.
        /// </summary>
        /// <remarks>
        /// Required. Maximum length of 200 characters. Used for identification in lists, detail pages, and reports.
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
        /// Maximum length of 1000 characters. Use for scope, notes, or additional context.
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
        /// Metadata only; does not store a file path. Maximum length of 500 characters.
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
        /// Project start date/time.
        /// </summary>
        /// <remarks>
        /// Optional. Defaults to current UTC time when not explicitly set. Prefer UTC/ISO-8601 in JSON.
        /// </remarks>
        /// <value>
        /// Date/time value or null.
        /// </value>
        /// <example>
        /// 2025-04-01T00:00:00Z
        /// </example>
        public DateTime? StartDate { get; set; } = DateTime.UtcNow;
    }
}