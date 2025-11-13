using Shared.Icp.DTOs.Common;

namespace Shared.Icp.DTOs.Elements
{
    /// <summary>
    /// Presentation DTO representing a calibration curve for an element within a project.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Used in API responses and detailed views to expose the regression parameters (slope/intercept),
    /// fit quality (<see cref="RSquared"/>), and related metadata for a specific element and project.
    /// Inherits from <see cref="BaseDto"/> and therefore includes common metadata such as
    /// <see cref="BaseDto.Id"/>, <see cref="BaseDto.CreatedAt"/>, and <see cref="BaseDto.UpdatedAt"/>.
    /// </para>
    /// <para>
    /// Notes:
    /// - Timestamps like <see cref="CalibrationDate"/> should be handled in UTC (ISO-8601 in JSON) to avoid time zone inconsistencies.
    /// - Units: concentration should be consistent across the curve (ppm, ppb, mg/L, etc.). Intensity is device-specific and
    ///   typically reported without a standard SI unit. Accordingly, <see cref="Slope"/> has units of (intensity)/(concentration),
    ///   and <see cref="Intercept"/> has units of intensity.
    /// - <see cref="RSquared"/> is expected within [0, 1]; values closer to 1 indicate a better fit.
    /// - User-facing messages elsewhere (validation/errors/success) remain Persian; this DTO only carries data.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var curve = new CalibrationCurveDto
    /// {
    ///     Id = Guid.NewGuid(),
    ///     CreatedAt = DateTime.UtcNow,
    ///     ElementId = Guid.Parse("3f2504e0-4f89-11d3-9a0c-0305e82c3301"),
    ///     ProjectId = Guid.Parse("7f9c2b4a-a1b2-4b3c-9d0e-112233445566"),
    ///     CalibrationDate = DateTime.UtcNow,
    ///     Slope = 12345.67m,   // intensity per unit concentration
    ///     Intercept = 12.34m,  // intensity
    ///     RSquared = 0.9987m,
    ///     Notes = "Linear fit using 7 standards (ppm)."
    /// };
    /// </code>
    /// </example>
    /// <seealso cref="CalibrationPointDto"/>
    /// <seealso cref="ElementDto"/>
    public class CalibrationCurveDto : BaseDto
    {
        /// <summary>
        /// Identifier of the element this calibration curve belongs to.
        /// </summary>
        /// <remarks>
        /// References the parent element for which the curve was established.
        /// </remarks>
        /// <value>
        /// GUID value.
        /// </value>
        /// <example>
        /// 3f2504e0-4f89-11d3-9a0c-0305e82c3301
        /// </example>
        public Guid ElementId { get; set; }

        /// <summary>
        /// Identifier of the project in which the calibration was performed.
        /// </summary>
        /// <remarks>
        /// Useful for scoping and retrieving the correct set of standards and measurements.
        /// </remarks>
        /// <value>
        /// GUID value.
        /// </value>
        /// <example>
        /// 7f9c2b4a-a1b2-4b3c-9d0e-112233445566
        /// </example>
        public Guid ProjectId { get; set; }

        /// <summary>
        /// Timestamp of the calibration activity.
        /// </summary>
        /// <remarks>
        /// Prefer UTC for storage/transfer (ISO-8601 in JSON).
        /// </remarks>
        /// <value>
        /// Date/time value.
        /// </value>
        /// <example>
        /// 2025-04-10T08:20:00Z
        /// </example>
        public DateTime CalibrationDate { get; set; }

        /// <summary>
        /// Slope of the calibration regression line.
        /// </summary>
        /// <remarks>
        /// Units: intensity per unit concentration (device-dependent). Typically non-negative.
        /// </remarks>
        /// <value>
        /// Decimal number.
        /// </value>
        /// <example>
        /// 12345.67
        /// </example>
        public decimal Slope { get; set; }

        /// <summary>
        /// Intercept of the calibration regression line.
        /// </summary>
        /// <remarks>
        /// Units: intensity (device-dependent). May be negative or positive depending on baseline correction.
        /// </remarks>
        /// <value>
        /// Decimal number.
        /// </value>
        /// <example>
        /// 12.34
        /// </example>
        public decimal Intercept { get; set; }

        /// <summary>
        /// Coefficient of determination (R²) for the calibration fit.
        /// </summary>
        /// <remarks>
        /// Expected range is [0, 1], where values closer to 1 indicate a better linear fit.
        /// </remarks>
        /// <value>
        /// Decimal number between 0 and 1.
        /// </value>
        /// <example>
        /// 0.9987
        /// </example>
        public decimal RSquared { get; set; }

        /// <summary>
        /// Optional notes regarding the calibration.
        /// </summary>
        /// <remarks>
        /// May include method details, instrument conditions, or remarks on standard preparation. Persian text is recommended for user presentation.
        /// </remarks>
        /// <value>
        /// String or null.
        /// </value>
        /// <example>
        /// Linear fit using 7 standards (ppm).
        /// </example>
        public string? Notes { get; set; }
    }
}