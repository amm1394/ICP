using Shared.Icp.DTOs.Common;
using Shared.Icp.DTOs.Elements;

namespace Shared.Icp.DTOs.Elements
{
    /// <summary>
    /// Presentation DTO representing a single calibration curve point.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Used in API responses and UI views to expose the concentration–intensity pair that forms one point
    /// on a calibration curve for a given element/analyte. Inherits from <see cref="BaseDto"/> and therefore
    /// includes common metadata such as <see cref="BaseDto.Id"/>, <see cref="BaseDto.CreatedAt"/>, and
    /// <see cref="BaseDto.UpdatedAt"/>.
    /// </para>
    /// <para>
    /// Notes:
    /// - Concentration units should be consistent across the curve (e.g., ppm, ppb, mg/L) and reflected in related UI.
    /// - Intensity is the instrument signal (often background-corrected) and is device-specific without a standard SI unit.
    /// - Points are typically ordered by increasing concentration (see <see cref="PointOrder"/>).
    /// - User-facing messages elsewhere (validation/errors/success) remain Persian; this DTO only carries data.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var point = new CalibrationPointDto
    /// {
    ///     Id = Guid.NewGuid(),
    ///     CreatedAt = DateTime.UtcNow,
    ///     CalibrationCurveId = Guid.Parse("3f2504e0-4f89-11d3-9a0c-0305e82c3301"),
    ///     Concentration = 10.0m, // ppm
    ///     Intensity = 125000m,   // instrument signal
    ///     PointOrder = 2
    /// };
    /// </code>
    /// </example>
    /// <seealso cref="CalibrationCurveDto"/>
    /// <seealso cref="ElementDto"/>
    public class CalibrationPointDto : BaseDto
    {
        /// <summary>
        /// شناسه منحنی کالیبراسیون
        /// </summary>
        /// <remarks>
        /// References the parent calibration curve resource.
        /// </remarks>
        /// <value>
        /// GUID value.
        /// </value>
        /// <example>
        /// 3f2504e0-4f89-11d3-9a0c-0305e82c3301
        /// </example>
        public Guid CalibrationCurveId { get; set; }

        /// <summary>
        /// غلظت
        /// </summary>
        /// <remarks>
        /// Use a consistent unit across the entire curve (e.g., ppm, ppb, mg/L). Must be non-negative.
        /// </remarks>
        /// <value>
        /// Non-negative decimal number.
        /// </value>
        /// <example>
        /// 10.0
        /// </example>
        public decimal Concentration { get; set; }

        /// <summary>
        /// شدت سیگنال
        /// </summary>
        /// <remarks>
        /// Device-dependent value (often background-corrected). Reported without a standardized SI unit.
        /// Must be non-negative.
        /// </remarks>
        /// <value>
        /// Non-negative decimal number.
        /// </value>
        /// <example>
        /// 125000
        /// </example>
        public decimal Intensity { get; set; }

        /// <summary>
        /// ترتیب نقطه
        /// </summary>
        /// <remarks>
        /// Typically starts from 1 and increases with concentration, but domain rules may vary.
        /// </remarks>
        /// <value>
        /// Positive integer.
        /// </value>
        /// <example>
        /// 2
        /// </example>
        public int PointOrder { get; set; }
    }
}