namespace Shared.Icp.DTOs.QualityControl
{
    /// <summary>
    /// Summary DTO aggregating quality control (QC) checks across a scope (project, batch, time window, etc.).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Designed for dashboards and reports to present high-level QC outcomes, including counts and
    /// a compact list of the most recent failed checks. User-facing messages elsewhere remain Persian;
    /// this type only carries data.
    /// </para>
    /// <para>
    /// Notes:
    /// - Totals should be consistent (TotalChecks = PassedChecks + FailedChecks + WarningChecks) unless your domain defines otherwise.
    /// - <see cref="ChecksByType"/> groups counts by QC category (e.g., "Blank", "CRM", "Duplicate").
    /// - <see cref="RecentFailures"/> is a convenience preview and is not guaranteed to be exhaustive.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var summary = new QualityCheckSummaryDto
    /// {
    ///     TotalChecks = 120,
    ///     PassedChecks = 108,
    ///     FailedChecks = 5,
    ///     WarningChecks = 7,
    ///     ChecksByType = new Dictionary<string, int>
    ///     {
    ///         ["Blank"] = 20,
    ///         ["CRM"] = 30,
    ///         ["Duplicate"] = 15,
    ///         ["Spike"] = 10,
    ///         ["Other"] = 45
    ///     },
    ///     RecentFailures = new List<QualityCheckDto>
    ///     {
    ///         new QualityCheckDto { /* ...populate minimal failure info... */ },
    ///         new QualityCheckDto { /* ... */ }
    ///     }
    /// };
    /// </code>
    /// </example>
    /// <seealso cref="QualityCheckDto"/>
    /// <seealso cref="QualityControl.QualityCheckResultDto"/>
    /// <seealso cref="QualityControl.BatchQualityCheckDto"/>
    public class QualityCheckSummaryDto
    {
        /// <summary>
        /// Total number of QC checks included in this summary.
        /// </summary>
        /// <remarks>
        /// May represent a project, a processing batch, or a time range depending on the query.
        /// </remarks>
        /// <value>
        /// Non-negative integer.
        /// </value>
        /// <example>
        /// 120
        /// </example>
        public int TotalChecks { get; set; }

        /// <summary>
        /// Number of QC checks that have passed according to domain rules.
        /// </summary>
        /// <value>
        /// Non-negative integer.
        /// </value>
        /// <example>
        /// 108
        /// </example>
        public int PassedChecks { get; set; }

        /// <summary>
        /// Number of QC checks that have failed.
        /// </summary>
        /// <remarks>
        /// Typically indicates the check is outside acceptable limits.
        /// </remarks>
        /// <value>
        /// Non-negative integer.
        /// </value>
        /// <example>
        /// 5
        /// </example>
        public int FailedChecks { get; set; }

        /// <summary>
        /// Number of QC checks that raised warnings.
        /// </summary>
        /// <remarks>
        /// Warnings usually indicate borderline or attention-required results that are not outright failures.
        /// </remarks>
        /// <value>
        /// Non-negative integer.
        /// </value>
        /// <example>
        /// 7
        /// </example>
        public int WarningChecks { get; set; }

        /// <summary>
        /// Aggregated counts of QC checks grouped by check type.
        /// </summary>
        /// <remarks>
        /// The dictionary key is the QC category name (e.g., "Blank", "CRM", "Duplicate"), and the value is the count in that category.
        /// </remarks>
        /// <value>
        /// Dictionary of check type to non-negative count. Initialized to an empty dictionary by default.
        /// </value>
        /// <example>
        /// { "Blank": 20, "CRM": 30, "Duplicate": 15 }
        /// </example>
        public Dictionary<string, int> ChecksByType { get; set; } = new();

        /// <summary>
        /// A compact list of the most recent failed QC checks.
        /// </summary>
        /// <remarks>
        /// Intended for quick diagnostics on dashboards; not necessarily a full historical record.
        /// May be empty when there are no recent failures.
        /// </remarks>
        /// <value>
        /// List of recent failed checks. Initialized to an empty list by default.
        /// </value>
        /// <example>
        /// [ { /* QualityCheckDto ... */ }, { /* ... */ } ]
        /// </example>
        public List<QualityCheckDto> RecentFailures { get; set; } = new();
    }
}