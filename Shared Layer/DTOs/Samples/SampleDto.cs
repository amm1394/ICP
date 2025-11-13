using Shared.Icp.DTOs.Common;

namespace Shared.Icp.DTOs.Samples
{
    /// <summary>
    /// Presentation DTO for returning a sample's information to clients.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Typically used in API responses to expose the current state of a sample with the required details.
    /// Inherits from <see cref="BaseDto"/>, therefore it also provides common metadata like
    /// <see cref="BaseDto.Id"/>, <see cref="BaseDto.CreatedAt"/>, and <see cref="BaseDto.UpdatedAt"/>.
    /// </para>
    /// <para>
    /// Notes:
    /// - Timestamps should be handled in UTC to avoid time zone inconsistencies (ISO-8601 in JSON).
    /// - Units are specified per field (grams, milliliters, and unitless factor).
    /// - Status strings should comply with your domain (e.g., "Pending", "InProgress", "Completed", "Rejected").
    /// - Consider domain-required precision for numeric values (e.g., 3–6 decimal places) on API/client sides.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Example of mapping a domain entity to SampleDto for an API response
    /// var dto = new SampleDto
    /// {
    ///     Id = Guid.NewGuid(),
    ///     CreatedAt = DateTime.UtcNow,
    ///     SampleId = "A-123-2025",
    ///     SampleName = "Sediment Sample",
    ///     RunDate = DateTime.UtcNow,
    ///     Status = "Completed",
    ///     Weight = 1.250m,        // grams
    ///     Volume = 50m,           // milliliters
    ///     DilutionFactor = 10m,   // unitless
    ///     Notes = "QC approved",
    ///     ProjectId = Guid.NewGuid()
    /// };
    /// </code>
    /// </example>
    /// <seealso cref="CreateSampleDto"/>
    /// <seealso cref="UpdateSampleDto"/>
    /// <seealso cref="MeasurementDto"/>
    public class SampleDto : BaseDto  // ✅ inherits from BaseDto
    {
        /// <summary>
        /// Human-readable/laboratory display identifier of the sample.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is the human-friendly code used on labels, reports, and lab workflows, and is different from the system-wide
        /// unique identifier <see cref="BaseDto.Id"/>. It is recommended to keep it unique according to your lab conventions.
        /// </para>
        /// </remarks>
        /// <value>
        /// Non-empty string. Example: "A-123-2025".
        /// </value>
        /// <example>
        /// A-123-2025
        /// </example>
        public string SampleId { get; set; } = string.Empty;

        /// <summary>
        /// Human-readable name of the sample.
        /// </summary>
        /// <remarks>
        /// Used for clearer identification in UIs and reports.
        /// </remarks>
        /// <value>
        /// Non-empty string.
        /// </value>
        /// <example>
        /// River Sediment
        /// </example>
        public string SampleName { get; set; } = string.Empty;

        /// <summary>
        /// The date and time the sample test was performed.
        /// </summary>
        /// <remarks>
        /// Prefer UTC for storage/transfer to avoid time zone inconsistencies. In JSON, typically ISO-8601.
        /// </remarks>
        /// <value>
        /// A valid date/time value.
        /// </value>
        /// <example>
        /// 2025-03-21T09:15:00Z
        /// </example>
        public DateTime RunDate { get; set; }

        /// <summary>
        /// Current status of the sample.
        /// </summary>
        /// <remarks>
        /// One of the valid domain values (e.g., "Pending", "InProgress", "Completed", "Rejected").
        /// </remarks>
        /// <value>
        /// Non-empty string.
        /// </value>
        /// <example>
        /// Completed
        /// </example>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Sample weight in grams.
        /// </summary>
        /// <remarks>
        /// Must be non-negative and, depending on domain rules, greater than zero. Unit: grams (g).
        /// </remarks>
        /// <value>
        /// Valid decimal number.
        /// </value>
        /// <example>
        /// 1.250
        /// </example>
        public decimal Weight { get; set; }

        /// <summary>
        /// Sample volume in milliliters.
        /// </summary>
        /// <remarks>
        /// Must be non-negative and, depending on domain rules, greater than zero. Unit: milliliters (mL).
        /// </remarks>
        /// <value>
        /// Valid decimal number.
        /// </value>
        /// <example>
        /// 50
        /// </example>
        public decimal Volume { get; set; }

        /// <summary>
        /// Sample dilution factor.
        /// </summary>
        /// <remarks>
        /// Unitless value typically greater than zero. Use 1 for no dilution.
        /// </remarks>
        /// <value>
        /// Valid decimal number (unitless).
        /// </value>
        /// <example>
        /// 10
        /// </example>
        public decimal DilutionFactor { get; set; }

        /// <summary>
        /// Free-form notes and additional details.
        /// </summary>
        /// <remarks>
        /// Use for QC notes, acceptance/rejection reasons, or any other relevant context.
        /// </remarks>
        /// <value>
        /// String or null.
        /// </value>
        /// <example>
        /// QC approved
        /// </example>
        public string? Notes { get; set; }

        /// <summary>
        /// Unique identifier of the project to which this sample belongs.
        /// </summary>
        /// <remarks>
        /// References the Project entity and is used for grouping and reporting.
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