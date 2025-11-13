namespace Shared.Icp.DTOs.QualityControl
{
    /// <summary>
    /// Request DTO for running batch quality control (QC) checks.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Used by API endpoints/services to trigger QC evaluation over a set of samples within a given project scope.
    /// The request specifies which samples and which QC check types to run, and optionally whether to stop at the
    /// first encountered failure.
    /// </para>
    /// <para>
    /// Localization: user-facing texts that may be produced elsewhere (e.g., QC messages) should remain Persian in
    /// presentation layers. This DTO carries only the input parameters and is language-agnostic.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var request = new BatchQualityCheckDto
    /// {
    ///     ProjectId = 42,
    ///     SampleIds = new List&lt;int&gt; { 101, 102, 103 },
    ///     CheckTypes = new List&lt;string&gt; { "Blank", "CRM", "Duplicate" },
    ///     StopOnFirstFailure = true
    /// };
    /// </code>
    /// </example>
    /// <seealso cref="QualityCheckDto"/>
    /// <seealso cref="QualityCheckResultDto"/>
    /// <seealso cref="QualityCheckSummaryDto"/>
    public class BatchQualityCheckDto
    {
        /// <summary>
        /// Numeric identifier of the project to evaluate.
        /// </summary>
        /// <remarks>
        /// Defines the scope within which QC checks are run. Authorization and data access rules typically derive from this scope.
        /// </remarks>
        /// <value>
        /// Positive integer.
        /// </value>
        /// <example>
        /// 42
        /// </example>
        public int ProjectId { get; set; }

        /// <summary>
        /// Collection of numeric sample identifiers to include in the QC run.
        /// </summary>
        /// <remarks>
        /// When empty, implementations may choose to evaluate all eligible samples in the specified project.
        /// Duplicate entries should be avoided; processing order is unspecified unless defined by the domain.
        /// </remarks>
        /// <value>
        /// List of positive integers. Initialized to an empty list by default.
        /// </value>
        /// <example>
        /// [ 101, 102, 103 ]
        /// </example>
        public List<int> SampleIds { get; set; } = new();

        /// <summary>
        /// QC check types to execute.
        /// </summary>
        /// <remarks>
        /// Common values include: "Blank", "CRM", "Duplicate", "Spike", "Recovery", etc. When empty, the service may run
        /// a default or domain-configured set of checks.
        /// </remarks>
        /// <value>
        /// List of non-empty strings. Initialized to an empty list by default.
        /// </value>
        /// <example>
        /// [ "Blank", "CRM", "Duplicate" ]
        /// </example>
        public List<string> CheckTypes { get; set; } = new();

        /// <summary>
        /// Whether to stop processing as soon as the first failure is encountered.
        /// </summary>
        /// <remarks>
        /// Useful for short-circuiting costly evaluations. Set to false to collect the full set of results. Default is false.
        /// </remarks>
        /// <value>
        /// Boolean value.
        /// </value>
        /// <example>
        /// true
        /// </example>
        public bool StopOnFirstFailure { get; set; } = false;
    }
}