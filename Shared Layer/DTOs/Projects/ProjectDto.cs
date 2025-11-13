namespace Shared.Icp.DTOs.Projects
{
    /// <summary>
    /// Presentation DTO exposing the full set of project information for client consumption.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Intended for detailed views and API responses where complete project metadata is required, including identity,
    /// temporal fields, status, and computed analytics (sample counts and progress).
    /// </para>
    /// <para>
    /// Notes:
    /// - Prefer UTC timestamps for all date/time fields (ISO-8601 in JSON) to avoid time zone inconsistencies.
    /// - Status values should follow your domain conventions (e.g., Active, Archived, Completed).
    /// - Computed properties are typically derived by the application/service layer and are read-only from a client perspective.
    /// - User-facing messages elsewhere (validation/errors/success) remain Persian; this DTO only carries data.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var project = new ProjectDto
    /// {
    ///     Id = Guid.NewGuid(),
    ///     Name = "River Monitoring Q2",
    ///     Description = "Quarterly assessment for the northern basin.",
    ///     SourceFileName = "import_q2_2025.xlsx",
    ///     Status = "Active",
    ///     StartDate = new DateTime(2025, 04, 01, 00, 00, 00, DateTimeKind.Utc),
    ///     EndDate = null,
    ///     CreatedAt = DateTime.UtcNow,
    ///     UpdatedAt = null,
    ///     SampleCount = 156,
    ///     ProcessedSamples = 120,
    ///     ApprovedSamples = 110,
    ///     RejectedSamples = 5,
    ///     PendingSamples = 36,
    ///     ProgressPercentage = 76.9,
    ///     DurationInDays = 60
    /// };
    /// </code>
    /// </example>
    /// <seealso cref="CreateProjectDto"/>
    /// <seealso cref="UpdateProjectDto"/>
    /// <seealso cref="ProjectSummaryDto"/>
    public class ProjectDto
    {
        /// <summary>
        /// Unique identifier of the project.
        /// </summary>
        /// <remarks>
        /// Immutable identifier used across services and storage.
        /// </remarks>
        /// <value>
        /// GUID value.
        /// </value>
        /// <example>
        /// 3f2504e0-4f89-11d3-9a0c-0305e82c3301
        /// </example>
        public Guid Id { get; set; }

        /// <summary>
        /// Project display name.
        /// </summary>
        /// <remarks>
        /// Shown in detailed views, lists, and reports.
        /// </remarks>
        /// <value>
        /// Non-empty string.
        /// </value>
        /// <example>
        /// River Monitoring Q2
        /// </example>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Optional project description.
        /// </summary>
        /// <remarks>
        /// Use for scope, notes, or additional context.
        /// </remarks>
        /// <value>
        /// String or null.
        /// </value>
        /// <example>
        /// Baseline survey focusing on heavy metals.
        /// </example>
        public string? Description { get; set; }

        /// <summary>
        /// Optional original source file name associated with the project.
        /// </summary>
        /// <remarks>
        /// Metadata only; does not reference a stored file path.
        /// </remarks>
        /// <value>
        /// String or null.
        /// </value>
        /// <example>
        /// import_q2_2025.xlsx
        /// </example>
        public string? SourceFileName { get; set; }

        /// <summary>
        /// Project status label.
        /// </summary>
        /// <remarks>
        /// Domain-defined value, typically something like Active, Archived, or Completed.
        /// </remarks>
        /// <value>
        /// Non-empty string.
        /// </value>
        /// <example>
        /// Active
        /// </example>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Project start date/time.
        /// </summary>
        /// <remarks>
        /// Prefer UTC for storage/transfer (ISO-8601 in JSON).
        /// </remarks>
        /// <value>
        /// Date/time value.
        /// </value>
        /// <example>
        /// 2025-04-01T00:00:00Z
        /// </example>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Optional project end date/time.
        /// </summary>
        /// <remarks>
        /// When null, the project has no defined end date.
        /// </remarks>
        /// <value>
        /// Date/time value or null.
        /// </value>
        /// <example>
        /// 2025-06-30T23:59:59Z
        /// </example>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Timestamp when the project was created.
        /// </summary>
        /// <remarks>
        /// Prefer UTC for storage/transfer (ISO-8601 in JSON).
        /// </remarks>
        /// <value>
        /// Date/time value.
        /// </value>
        /// <example>
        /// 2025-03-15T12:34:56Z
        /// </example>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Timestamp when the project was last updated.
        /// </summary>
        /// <remarks>
        /// Null when the project has not been modified since creation. Prefer UTC (ISO-8601 in JSON).
        /// </remarks>
        /// <value>
        /// Date/time value or null.
        /// </value>
        /// <example>
        /// 2025-04-10T08:20:00Z
        /// </example>
        public DateTime? UpdatedAt { get; set; }

        // Computed Properties

        /// <summary>
        /// Total number of samples associated with the project.
        /// </summary>
        /// <remarks>
        /// May be filtered by query context. Non-negative value.
        /// </remarks>
        /// <value>
        /// Non-negative integer.
        /// </value>
        /// <example>
        /// 156
        /// </example>
        public int SampleCount { get; set; }

        /// <summary>
        /// Number of samples that have been processed.
        /// </summary>
        /// <remarks>
        /// Should not exceed <see cref="SampleCount"/>.
        /// </remarks>
        /// <value>
        /// Non-negative integer.
        /// </value>
        /// <example>
        /// 120
        /// </example>
        public int ProcessedSamples { get; set; }

        /// <summary>
        /// Number of samples approved according to domain rules.
        /// </summary>
        /// <remarks>
        /// Should not exceed <see cref="ProcessedSamples"/>.
        /// </remarks>
        /// <value>
        /// Non-negative integer.
        /// </value>
        /// <example>
        /// 110
        /// </example>
        public int ApprovedSamples { get; set; }

        /// <summary>
        /// Number of samples rejected according to domain rules.
        /// </summary>
        /// <remarks>
        /// Should not exceed <see cref="ProcessedSamples"/>.
        /// </remarks>
        /// <value>
        /// Non-negative integer.
        /// </value>
        /// <example>
        /// 5
        /// </example>
        public int RejectedSamples { get; set; }

        /// <summary>
        /// Number of samples pending processing or review.
        /// </summary>
        /// <remarks>
        /// Depending on domain rules, may be derived as <c>SampleCount - ProcessedSamples</c> or include other states.
        /// </remarks>
        /// <value>
        /// Non-negative integer.
        /// </value>
        /// <example>
        /// 36
        /// </example>
        public int PendingSamples { get; set; }

        /// <summary>
        /// Overall processing progress percentage for the project.
        /// </summary>
        /// <remarks>
        /// Typically within the [0, 100] range. Calculation is domain-defined (e.g., based on processed vs. total samples).
        /// </remarks>
        /// <value>
        /// Double precision value.
        /// </value>
        /// <example>
        /// 76.9
        /// </example>
        public double ProgressPercentage { get; set; }

        /// <summary>
        /// Project duration in days, if applicable.
        /// </summary>
        /// <remarks>
        /// Often calculated from <see cref="StartDate"/> to <see cref="EndDate"/> (or to the current date if ongoing). Null when not applicable.
        /// </remarks>
        /// <value>
        /// Integer value or null.
        /// </value>
        /// <example>
        /// 60
        /// </example>
        public int? DurationInDays { get; set; }
    }
}