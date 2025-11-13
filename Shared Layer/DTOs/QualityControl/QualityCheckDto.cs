using Shared.Icp.DTOs.Common;

namespace Shared.Icp.DTOs.QualityControl
{
    /// <summary>
    /// Presentation DTO describing a single quality control (QC) check instance for a sample.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Inherits from <see cref="BaseDto"/> to provide common metadata (Id/CreatedAt/UpdatedAt). Intended for use in
    /// API responses, dashboards, and reports to show the outcome and details of a QC check performed on a sample.
    /// </para>
    /// <para>
    /// Localization: user-facing texts (e.g., <see cref="Message"/>) should be provided in Persian for presentation.
    /// This DTO itself remains language-agnostic and only carries data.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var qc = new QualityCheckDto
    /// {
    ///     Id = Guid.NewGuid(),
    ///     CreatedAt = DateTime.UtcNow,
    ///     SampleId = 101,
    ///     SampleName = "Well A Water",
    ///     CheckType = "CRM",
    ///     Status = "Failed",
    ///     ExpectedValue = 100.0m,
    ///     MeasuredValue = 102.15m,
    ///     Deviation = 2.15m,                // domain-defined (e.g., absolute or %)
    ///     AcceptableDeviationLimit = 2.00m,  // domain-defined
    ///     CheckDate = DateTime.UtcNow,
    ///     Message = "انحراف از مقدار مرجع بیشتر از حد مجاز است."
    /// };
    /// </code>
    /// </example>
    /// <seealso cref="QualityCheckResultDto"/>
    /// <seealso cref="QualityCheckSummaryDto"/>
    public class QualityCheckDto : BaseDto
    {
        /// <summary>
        /// Numeric identifier of the sample that this QC check refers to.
        /// </summary>
        /// <remarks>
        /// Different from any human-readable laboratory sample code. For display purposes, see <see cref="SampleName"/>.
        /// </remarks>
        /// <value>
        /// Positive integer.
        /// </value>
        /// <example>
        /// 101
        /// </example>
        public int SampleId { get; set; }

        /// <summary>
        /// Human-readable name of the sample.
        /// </summary>
        /// <remarks>
        /// Used for UI/report readability alongside the numeric <see cref="SampleId"/>.
        /// </remarks>
        /// <value>
        /// Non-empty string.
        /// </value>
        /// <example>
        /// Well A Water
        /// </example>
        public string SampleName { get; set; } = string.Empty;

        /// <summary>
        /// Type/category of the QC check.
        /// </summary>
        /// <remarks>
        /// Common values include: "Blank", "CRM", "Duplicate", "Spike", "Recovery", etc.
        /// </remarks>
        /// <value>
        /// Non-empty string.
        /// </value>
        /// <example>
        /// CRM
        /// </example>
        public string CheckType { get; set; } = string.Empty;

        /// <summary>
        /// High-level status of the QC check outcome.
        /// </summary>
        /// <remarks>
        /// Domain-defined values, typically one of: "Passed", "Failed", or "Warning".
        /// </remarks>
        /// <value>
        /// Non-empty string.
        /// </value>
        /// <example>
        /// Failed
        /// </example>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// The expected/target value used by the QC rule.
        /// </summary>
        /// <remarks>
        /// The semantics and units are domain-specific (e.g., mg/L, ppm, or device unit). Should be comparable with <see cref="MeasuredValue"/>.
        /// </remarks>
        /// <value>
        /// Decimal number or null.
        /// </value>
        /// <example>
        /// 100.0
        /// </example>
        public decimal? ExpectedValue { get; set; }

        /// <summary>
        /// The measured/observed value for the QC check.
        /// </summary>
        /// <remarks>
        /// Unit and scale must match <see cref="ExpectedValue"/> to allow meaningful comparison.
        /// </remarks>
        /// <value>
        /// Decimal number or null.
        /// </value>
        /// <example>
        /// 102.15
        /// </example>
        public decimal? MeasuredValue { get; set; }

        /// <summary>
        /// Deviation calculated by the QC rule (e.g., measured - expected, or percentage).
        /// </summary>
        /// <remarks>
        /// The formula and unit are domain-defined. Positive/negative values may indicate direction of deviation.
        /// </remarks>
        /// <value>
        /// Decimal number or null.
        /// </value>
        /// <example>
        /// 2.15
        /// </example>
        public decimal? Deviation { get; set; }

        /// <summary>
        /// Acceptable deviation limit/threshold for the QC rule.
        /// </summary>
        /// <remarks>
        /// Comparison logic is domain-specific (e.g., absolute vs percentage, inclusive vs exclusive threshold).
        /// </remarks>
        /// <value>
        /// Decimal number or null.
        /// </value>
        /// <example>
        /// 2.00
        /// </example>
        public decimal? AcceptableDeviationLimit { get; set; }

        /// <summary>
        /// Timestamp when the QC check was performed.
        /// </summary>
        /// <remarks>
        /// Prefer UTC for storage/transfer (ISO-8601 in JSON) to avoid time zone inconsistencies.
        /// </remarks>
        /// <value>
        /// Valid date/time value.
        /// </value>
        /// <example>
        /// 2025-03-21T10:30:00Z
        /// </example>
        public DateTime CheckDate { get; set; }

        /// <summary>
        /// User-facing explanatory message for this QC result.
        /// </summary>
        /// <remarks>
        /// Should be localized in Persian for end users. May include brief guidance about the result.
        /// </remarks>
        /// <value>
        /// String or null.
        /// </value>
        /// <example>
        /// انحراف از مقدار مرجع بیشتر از حد مجاز است.
        /// </example>
        public string? Message { get; set; }
    }
}