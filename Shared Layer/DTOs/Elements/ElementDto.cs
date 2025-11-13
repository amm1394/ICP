using Shared.Icp.DTOs.Common;

namespace Shared.Icp.DTOs.Elements
{
    /// <summary>
    /// Presentation DTO representing a chemical element for client consumption.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Inherits from <see cref="BaseDto"/> and therefore includes common metadata such as
    /// <see cref="BaseDto.Id"/>, <see cref="BaseDto.CreatedAt"/>, and <see cref="BaseDto.UpdatedAt"/>.
    /// Intended for API responses and UI views that require element-level information.
    /// </para>
    /// <para>
    /// Notes:
    /// - <see cref="AtomicNumber"/> is the number of protons in the nucleus (positive integer).
    /// - <see cref="AtomicMass"/> is reported in unified atomic mass units (u); consider domain-required precision.
    /// - <see cref="IsActive"/> controls whether the element is available for selection/processing.
    /// - <see cref="DisplayOrder"/> can be used to sort elements in user interfaces (lower appears first).
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var element = new ElementDto
    /// {
    ///     Id = Guid.NewGuid(),
    ///     CreatedAt = DateTime.UtcNow,
    ///     Symbol = "Cu",
    ///     Name = "Copper",
    ///     AtomicNumber = 29,
    ///     AtomicMass = 63.546m, // u
    ///     IsActive = true,
    ///     DisplayOrder = 10
    /// };
    /// </code>
    /// </example>
    /// <seealso cref="IsotopeDto"/>
    /// <seealso cref="CreateElementDto"/>
    /// <seealso cref="UpdateElementDto"/>
    public class ElementDto : BaseDto
    {
        /// <summary>
        /// Chemical symbol of the element (e.g., H, He, Fe, Cu).
        /// </summary>
        /// <value>
        /// Non-empty string.
        /// </value>
        /// <example>
        /// Cu
        /// </example>
        public string Symbol { get; set; } = string.Empty;

        /// <summary>
        /// Display name of the element.
        /// </summary>
        /// <remarks>
        /// Typically the English name (e.g., Copper). For localized/Persian display, map in the presentation layer if needed.
        /// </remarks>
        /// <value>
        /// Non-empty string.
        /// </value>
        /// <example>
        /// Copper
        /// </example>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Atomic number (Z) of the element.
        /// </summary>
        /// <remarks>
        /// Equals the number of protons in the nucleus. Must be a positive integer.
        /// </remarks>
        /// <value>
        /// Positive integer.
        /// </value>
        /// <example>
        /// 29
        /// </example>
        public int AtomicNumber { get; set; }

        /// <summary>
        /// Atomic mass of the element in unified atomic mass units (u).
        /// </summary>
        /// <remarks>
        /// Should be non-negative and use appropriate precision per domain rules.
        /// </remarks>
        /// <value>
        /// Decimal number (unit: u).
        /// </value>
        /// <example>
        /// 63.546
        /// </example>
        public decimal AtomicMass { get; set; }

        /// <summary>
        /// Activation flag indicating whether the element is active.
        /// </summary>
        /// <remarks>
        /// When false, the element may be hidden or disabled in selection lists.
        /// </remarks>
        /// <value>
        /// Boolean value.
        /// </value>
        /// <example>
        /// true
        /// </example>
        public bool IsActive { get; set; }

        /// <summary>
        /// Display order used to sort elements in UI lists.
        /// </summary>
        /// <remarks>
        /// Lower values typically appear earlier. Should be non-negative.
        /// </remarks>
        /// <value>
        /// Non-negative integer.
        /// </value>
        /// <example>
        /// 10
        /// </example>
        public int DisplayOrder { get; set; }
    }
}