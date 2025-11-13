using Shared.Icp.DTOs.Common;

namespace Shared.Icp.DTOs.Samples
{
    /// <summary>
    /// Presentation DTO representing a measurement associated with a specific sample and element.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This model exposes processed/derived measurement results for each Sample–Element pair (and, if applicable, a specific isotope).
    /// It inherits from <see cref="BaseDto"/>, therefore it also carries the common metadata such as
    /// <see cref="BaseDto.Id"/>, <see cref="BaseDto.CreatedAt"/>, and <see cref="BaseDto.UpdatedAt"/>.
    /// </para>
    /// <para>
    /// Notes:
    /// - Timestamps should be handled in UTC to avoid time zone inconsistencies.
    /// - The default concentration unit is "ppm" unless overridden by <see cref="Unit"/>.
    /// - Consider the precision required by the domain (e.g., 3–6 decimal places) for numeric values when serializing.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var m = new MeasurementDto
    /// {
    ///     Id = Guid.NewGuid(),
    ///     CreatedAt = DateTime.UtcNow,
    ///     SampleId = 101,
    ///     SampleName = "Well A Water",
    ///     ElementId = 14,
    ///     ElementSymbol = "Si",
    ///     ElementName = "Silicon",
    ///     Isotope = 28,
    ///     NetIntensity = 12345.678m,
    ///     Concentration = 2.345m,      // ppm
    ///     Unit = "ppm",
    ///     FinalConcentration = 23.45m, // after dilution factor
    ///     StandardError = 0.015m,
    ///     IsValid = true,
    ///     Notes = "QC Passed"
    /// };
    /// </code>
    /// </example>
    /// <seealso cref="SampleDto"/>
    /// <seealso cref="ElementDto"/>
    /// <seealso cref="IsotopeDto"/>
    public class MeasurementDto : BaseDto
    {
        /// <summary>
        /// Numeric identifier of the sample this measurement belongs to.
        /// </summary>
        /// <remarks>
        /// This is different from the DTO's GUID <see cref="BaseDto.Id"/>. For display details of the sample, see <see cref="SampleDto"/>.
        /// </remarks>
        /// <value>
        /// Positive integer.
        /// </value>
        /// <example>
        /// 101
        /// </example>
        public int SampleId { get; set; }

        /// <summary>
        /// Human-readable name of the related sample.
        /// </summary>
        /// <remarks>
        /// Used for user interface and reporting.
        /// </remarks>
        /// <value>
        /// Non-empty string.
        /// </value>
        /// <example>
        /// Well A Water
        /// </example>
        public string SampleName { get; set; } = string.Empty;

        /// <summary>
        /// Numeric identifier of the measured element.
        /// </summary>
        /// <remarks>
        /// Refers to the Element entity. For details, see <see cref="ElementDto"/>.
        /// </remarks>
        /// <value>
        /// Positive integer.
        /// </value>
        /// <example>
        /// 14
        /// </example>
        public int ElementId { get; set; }

        /// <summary>
        /// Chemical symbol of the element (e.g., Cu, Fe, Si).
        /// </summary>
        /// <value>
        /// Non-empty string.
        /// </value>
        /// <example>
        /// Si
        /// </example>
        public string ElementSymbol { get; set; } = string.Empty;

        /// <summary>
        /// Full name of the element.
        /// </summary>
        /// <value>
        /// Non-empty string.
        /// </value>
        /// <example>
        /// Silicon
        /// </example>
        public string ElementName { get; set; } = string.Empty;

        /// <summary>
        /// Measured/used isotope mass number, if any.
        /// </summary>
        /// <remarks>
        /// In certain techniques (e.g., ICP-MS), specifying the mass number is important. When null, the measurement is isotope-agnostic.
        /// </remarks>
        /// <value>
        /// Positive integer or null. Example: 63 for Copper.
        /// </value>
        /// <example>
        /// 28
        /// </example>
        public int? Isotope { get; set; }

        /// <summary>
        /// Background-corrected signal intensity.
        /// </summary>
        /// <remarks>
        /// A raw device-dependent value (background subtracted) typically used to compute concentration. Reported without a standardized physical unit.
        /// </remarks>
        /// <value>
        /// Non-negative decimal number.
        /// </value>
        /// <example>
        /// 12345.678
        /// </example>
        public decimal NetIntensity { get; set; }

        /// <summary>
        /// Calculated concentration before applying dilution.
        /// </summary>
        /// <remarks>
        /// Derived from the calibration curve. The unit is defined by <see cref="Unit"/> (default is ppm).
        /// </remarks>
        /// <value>
        /// Decimal number or null.
        /// </value>
        /// <example>
        /// 2.345
        /// </example>
        public decimal? Concentration { get; set; }

        /// <summary>
        /// Unit of measure for concentration.
        /// </summary>
        /// <remarks>
        /// Default is "ppm". Other common values include "ppb" and "mg/L" depending on domain needs.
        /// </remarks>
        /// <value>
        /// Non-empty string.
        /// </value>
        /// <example>
        /// ppm
        /// </example>
        public string Unit { get; set; } = "ppm";

        /// <summary>
        /// Final concentration after applying the sample dilution factor.
        /// </summary>
        /// <remarks>
        /// Typically equals <see cref="Concentration"/> multiplied by the sample's dilution factor. Same unit as <see cref="Unit"/>.
        /// </remarks>
        /// <value>
        /// Decimal number or null.
        /// </value>
        /// <example>
        /// 23.45
        /// </example>
        public decimal? FinalConcentration { get; set; }

        /// <summary>
        /// Standard error of the measurement.
        /// </summary>
        /// <remarks>
        /// May be computed from replicate measurements. The unit should match the concentration unit.
        /// </remarks>
        /// <value>
        /// Decimal number or null.
        /// </value>
        /// <example>
        /// 0.015
        /// </example>
        public decimal? StandardError { get; set; }

        /// <summary>
        /// Quality control validity flag.
        /// </summary>
        /// <remarks>
        /// True when the measurement passes QC checks (e.g., blanks, control standards, repeatability).
        /// </remarks>
        /// <value>
        /// Boolean value.
        /// </value>
        /// <example>
        /// true
        /// </example>
        public bool IsValid { get; set; }

        /// <summary>
        /// Free-form notes and additional context for this measurement.
        /// </summary>
        /// <remarks>
        /// May include reasons for invalidation, special test conditions, or device-related remarks.
        /// </remarks>
        /// <value>
        /// String or null.
        /// </value>
        /// <example>
        /// QC Passed
        /// </example>
        public string? Notes { get; set; }
    }
}