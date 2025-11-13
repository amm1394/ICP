namespace Shared.Icp.DTOs.Elements
{
    /// <summary>
    /// DTO used to create a new chemical element.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Intended for POST operations in the API layer. Carries the input data required to register a new element.
    /// Validation and additional domain rules (e.g., uniqueness of <see cref="Symbol"/>, positive ranges) should be enforced
    /// by the application/domain layer. User-facing messages elsewhere remain Persian; this DTO is language-agnostic.
    /// </para>
    /// <para>
    /// Notes:
    /// - <see cref="AtomicNumber"/> must be a positive integer (proton count).
    /// - <see cref="AtomicMass"/> is in unified atomic mass units (u); apply domain-required precision.
    /// - <see cref="DisplayOrder"/> can be used by UIs to sort elements; lower numbers typically appear first.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new CreateElementDto
    /// {
    ///     Symbol = "Ce",
    ///     Name = "Cerium",        // or localized Persian name in presentation needs
    ///     AtomicNumber = 58,
    ///     AtomicMass = 140.116m,   // u
    ///     DisplayOrder = 10
    /// };
    /// </code>
    /// </example>
    /// <seealso cref="ElementDto"/>
    /// <seealso cref="UpdateElementDto"/>
    /// <seealso cref="IsotopeDto"/>
    public class CreateElementDto
    {
        /// <summary>
        /// Chemical symbol of the element (e.g., Ce, La, Nd).
        /// </summary>
        /// <remarks>
        /// Commonly the IUPAC symbol. Should be unique across elements.
        /// </remarks>
        /// <value>
        /// Non-empty string.
        /// </value>
        /// <example>
        /// Ce
        /// </example>
        public string Symbol { get; set; } = string.Empty;

        /// <summary>
        /// Display name of the element.
        /// </summary>
        /// <remarks>
        /// Typically the English name (e.g., Copper) or a localized Persian name for presentation.
        /// </remarks>
        /// <value>
        /// Non-empty string.
        /// </value>
        /// <example>
        /// Cerium
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
        /// 58
        /// </example>
        public int AtomicNumber { get; set; }

        /// <summary>
        /// Atomic mass in unified atomic mass units (u).
        /// </summary>
        /// <remarks>
        /// Should be non-negative and use an appropriate precision per domain rules.
        /// </remarks>
        /// <value>
        /// Decimal number (unit: u).
        /// </value>
        /// <example>
        /// 140.116
        /// </example>
        public decimal AtomicMass { get; set; }

        /// <summary>
        /// Optional display order for sorting elements in UI.
        /// </summary>
        /// <remarks>
        /// Lower values typically appear earlier. Must be non-negative when provided.
        /// </remarks>
        /// <value>
        /// Non-negative integer or null.
        /// </value>
        /// <example>
        /// 10
        /// </example>
        public int? DisplayOrder { get; set; }
    }
}