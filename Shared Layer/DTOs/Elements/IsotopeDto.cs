using Shared.Icp.DTOs.Common;

namespace Shared.Icp.DTOs.Elements
{
    /// <summary>
    /// Presentation DTO representing an isotope of a chemical element.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Inherits from <see cref="BaseDto"/> to provide common metadata (<see cref="BaseDto.Id"/>,
    /// <see cref="BaseDto.CreatedAt"/>, <see cref="BaseDto.UpdatedAt"/>). Intended for API responses and UI views
    /// that require isotope-level details for a given element.
    /// </para>
    /// <para>
    /// Localization: user-facing messages elsewhere remain Persian; this DTO only carries data and is language-agnostic.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var iso = new IsotopeDto
    /// {
    ///     Id = Guid.NewGuid(),
    ///     CreatedAt = DateTime.UtcNow,
    ///     ElementId = Guid.Parse("3f2504e0-4f89-11d3-9a0c-0305e82c3301"),
    ///     MassNumber = 63,
    ///     Abundance = 69.17m,   // percent
    ///     IsStable = true
    /// };
    /// </code>
    /// </example>
    /// <seealso cref="ElementDto"/>
    public class IsotopeDto : BaseDto
    {
        /// <summary>
        /// Identifier of the element this isotope belongs to.
        /// </summary>
        /// <remarks>
        /// References the parent <c>Element</c> entity. Used to group isotopes under their element.
        /// </remarks>
        /// <value>
        /// GUID value.
        /// </value>
        /// <example>
        /// 3f2504e0-4f89-11d3-9a0c-0305e82c3301
        /// </example>
        public Guid ElementId { get; set; }

        /// <summary>
        /// Mass number (A) of the isotope.
        /// </summary>
        /// <remarks>
        /// Represents the total number of protons and neutrons. Must be a positive integer.
        /// </remarks>
        /// <value>
        /// Positive integer.
        /// </value>
        /// <example>
        /// 63
        /// </example>
        public int MassNumber { get; set; }

        /// <summary>
        /// Natural isotopic abundance in percent.
        /// </summary>
        /// <remarks>
        /// Expressed as a percentage within [0, 100]. Ensure consistency with domain expectations (percent vs fraction).
        /// </remarks>
        /// <value>
        /// Decimal value between 0 and 100.
        /// </value>
        /// <example>
        /// 69.17
        /// </example>
        public decimal Abundance { get; set; }

        /// <summary>
        /// Indicates whether the isotope is stable.
        /// </summary>
        /// <remarks>
        /// True for stable isotopes; false for radioactive/unstable ones.
        /// </remarks>
        /// <value>
        /// Boolean value.
        /// </value>
        /// <example>
        /// true
        /// </example>
        public bool IsStable { get; set; }
    }
}