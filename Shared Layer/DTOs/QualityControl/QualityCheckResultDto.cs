namespace Shared.Icp.DTOs.QualityControl
{
    /// <summary>
    /// DTO representing the result of a single quality control (QC) check.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Intended to convey the outcome of a QC rule, including pass/fail state, status label, a user-facing message,
    /// numeric deviation (if applicable), and any warning/error messages collected during evaluation.
    /// </para>
    /// <para>
    /// Localization: user-facing texts (e.g., <see cref="Message"/>, items in <see cref="Warnings"/>, <see cref="Errors"/>)
    /// should be provided in Persian for presentation. This DTO itself remains language-agnostic.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = new QualityCheckResultDto
    /// {
    ///     Passed = false,
    ///     CheckType = "CRM",
    ///     Status = "Failed",
    ///     Message = "خطای QC: انحراف از مقدار مرجع بیشتر از حد مجاز است.",
    ///     Deviation = 2.15m, // domain-defined (e.g., absolute or %)
    ///     Warnings = new List<string> { "نزدیک به حد آستانه" },
    ///     Errors = new List<string> { "کالیبراسیون قدیمی است" }
    /// };
    /// </code>
    /// </example>
    /// <seealso cref="QualityCheckSummaryDto"/>
    /// <seealso cref="QualityCheckDto"/>
    public class QualityCheckResultDto
    {
        /// <summary>
        /// Indicates whether the QC check passed according to domain rules.
        /// </summary>
        /// <value>
        /// Boolean value: true for pass; false for fail or warning depending on implementation.
        /// </value>
        /// <example>
        /// true
        /// </example>
        public bool Passed { get; set; }

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
        /// High-level status label for the QC check result.
        /// </summary>
        /// <remarks>
        /// Typically one of: "Passed", "Failed", "Warning" (or a domain-specific equivalent).
        /// </remarks>
        /// <value>
        /// Non-empty string.
        /// </value>
        /// <example>
        /// Failed
        /// </example>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// User-facing message describing the result.
        /// </summary>
        /// <remarks>
        /// Should be localized in Persian for display to end users. May include brief guidance on next steps.
        /// </remarks>
        /// <value>
        /// String; empty when not applicable.
        /// </value>
        /// <example>
        /// خطای QC: انحراف از مقدار مرجع بیشتر از حد مجاز است.
        /// </example>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Numeric deviation from the expected or target value, when applicable.
        /// </summary>
        /// <remarks>
        /// Semantics (absolute vs. percentage) are domain-defined. Ensure the unit or scale is consistent with the check type.
        /// Positive or negative values may indicate direction of deviation.
        /// </remarks>
        /// <value>
        /// Decimal value or null.
        /// </value>
        /// <example>
        /// 2.15
        /// </example>
        public decimal? Deviation { get; set; }

        /// <summary>
        /// A collection of warning messages related to the QC evaluation.
        /// </summary>
        /// <remarks>
        /// Messages should be localized in Persian for presentation. The list may be empty when there are no warnings.
        /// </remarks>
        /// <value>
        /// List of strings; initialized to an empty list.
        /// </value>
        /// <example>
        /// [ "نزدیک به حد آستانه" ]
        /// </example>
        public List<string> Warnings { get; set; } = new();

        /// <summary>
        /// A collection of error messages encountered during the QC evaluation.
        /// </summary>
        /// <remarks>
        /// Messages should be localized in Persian for presentation. The list may be empty when there are no errors.
        /// </remarks>
        /// <value>
        /// List of strings; initialized to an empty list.
        /// </value>
        /// <example>
        /// [ "کالیبراسیون قدیمی است" ]
        /// </example>
        public List<string> Errors { get; set; } = new();
    }
}