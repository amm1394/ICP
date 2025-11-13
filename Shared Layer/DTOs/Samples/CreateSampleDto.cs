namespace Shared.Icp.DTOs.Samples
{
    /// <summary>
    /// DTO used to create a new sample.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Intended for POST create operations in the API layer. It carries the input data required to create a sample
    /// and may be validated by the domain or controller. Unless otherwise noted, properties are optional, while
    /// <see cref="ProjectId"/> is required to associate the sample with a project.
    /// </para>
    /// <para>
    /// Defaults/Notes:
    /// - If <see cref="RunDate"/> is not provided, the server may use the current time.
    /// - If <see cref="DilutionFactor"/> is not provided, the logical default is typically 1 (no dilution).
    /// - Units are specified per field (grams, milliliters, and unitless factor).
    /// - Use UTC timestamps (ISO-8601 in JSON) to avoid time zone inconsistencies.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Example payload for creating a new sample
    /// var dto = new CreateSampleDto
    /// {
    ///     SampleId = "A-123-2025",   // laboratory display ID (optional)
    ///     SampleName = "River Sediment",
    ///     RunDate = DateTime.UtcNow,  // optional; server may default to now
    ///     Weight = 1.250m,            // grams (optional)
    ///     Volume = 50m,               // milliliters (optional)
    ///     DilutionFactor = 10m,       // unitless (optional; default 1)
    ///     Notes = "Collected near station 7",
    ///     ProjectId = Guid.NewGuid()  // required
    /// };
    /// </code>
    /// </example>
    /// <seealso cref="SampleDto"/>
    /// <seealso cref="UpdateSampleDto"/>
    public class CreateSampleDto
    {
        /// <summary>
        /// Human-readable laboratory Sample ID (display identifier).
        /// </summary>
        /// <remarks>
        /// This value is used on labels, reports, and lab workflows. It differs from any internal system GUID.
        /// </remarks>
        /// <value>
        /// String value; optional. Example: "A-123-2025".
        /// </value>
        /// <example>
        /// A-123-2025
        /// </example>
        public string SampleId { get; set; } = string.Empty;

        /// <summary>
        /// Human-readable name of the sample.
        /// </summary>
        /// <remarks>
        /// Used for clearer identification in user interfaces and reports.
        /// </remarks>
        /// <value>
        /// Non-empty string is recommended; domain rules may require it.
        /// </value>
        /// <example>
        /// River Sediment
        /// </example>
        public string SampleName { get; set; } = string.Empty;

        /// <summary>
        /// Test execution timestamp.
        /// </summary>
        /// <remarks>
        /// Optional. When omitted, the server may use the current time. Prefer UTC (ISO-8601 in JSON).
        /// </remarks>
        /// <value>
        /// Date/time value or null.
        /// </value>
        /// <example>
        /// 2025-03-21T09:15:00Z
        /// </example>
        public DateTime? RunDate { get; set; }

        /// <summary>
        /// Sample weight in grams.
        /// </summary>
        /// <remarks>
        /// Optional. Must be non-negative and, depending on domain rules, greater than zero. Unit: grams (g).
        /// </remarks>
        /// <value>
        /// Decimal number or null.
        /// </value>
        /// <example>
        /// 1.250
        /// </example>
        public decimal? Weight { get; set; }

        /// <summary>
        /// Sample volume in milliliters.
        /// </summary>
        /// <remarks>
        /// Optional. Must be non-negative and, depending on domain rules, greater than zero. Unit: milliliters (mL).
        /// </remarks>
        /// <value>
        /// Decimal number or null.
        /// </value>
        /// <example>
        /// 50
        /// </example>
        public decimal? Volume { get; set; }

        /// <summary>
        /// Dilution factor applied to the sample.
        /// </summary>
        /// <remarks>
        /// Optional. Unitless value typically greater than zero. Default is 1 (no dilution).
        /// </remarks>
        /// <value>
        /// Decimal number or null (unitless).
        /// </value>
        /// <example>
        /// 10
        /// </example>
        public decimal? DilutionFactor { get; set; }

        /// <summary>
        /// Free-form notes and additional context.
        /// </summary>
        /// <remarks>
        /// Optional. Use for QC notes, collection conditions, or any other relevant details.
        /// </remarks>
        /// <value>
        /// String or null.
        /// </value>
        /// <example>
        /// Collected near station 7
        /// </example>
        public string? Notes { get; set; }

        /// <summary>
        /// Unique identifier of the project the sample belongs to.
        /// </summary>
        /// <remarks>
        /// Required. References the Project entity and is used for grouping, authorization, and reporting.
        /// </remarks>
        /// <value>
        /// A valid GUID value.
        /// </value>
        /// <example>
        /// 3f2504e0-4f89-11d3-9a0c-0305e82c3301
        /// </example>
        public Guid ProjectId { get; set; }
    }
}