namespace Shared.Icp.DTOs.Elements
{
    /// <summary>
    /// DTO used to update an existing chemical element.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Intended for PUT/PATCH operations in the API layer. It carries only mutable fields so that updates can be
    /// partial: when a property is not provided (null for nullable types), the server may ignore it and keep the
    /// current value according to domain rules.
    /// </para>
    /// <para>
    /// Localization: any user-facing messages produced elsewhere should remain Persian in presentation layers;
    /// this DTO is language-agnostic and only transports data.
    /// </para>
    /// <para>
    /// Notes:
    /// - <see cref="AtomicMass"/> uses the unified atomic mass unit (u). Consider domain-required precision (e.g., 4–6 decimals).
    /// - <see cref="IsActive"/> toggles whether the element is available for selection/processing.
    /// - <see cref="DisplayOrder"/> can be used to sort elements in UIs; lower numbers typically appear first.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new UpdateElementDto
    /// {
    ///     Name = "مس",            // Persian display name (optional)
    ///     AtomicMass = 63.546m,    // u (optional)
    ///     IsActive = true,         // optional
    ///     DisplayOrder = 10        // optional
    /// };
    /// </code>
    /// </example>
    /// <seealso cref="ElementDto"/>
    /// <seealso cref="CreateElementDto"/>
    /// <seealso cref="IsotopeDto"/>
    public class UpdateElementDto
    {
        /// <summary>
        /// Persian display name of the element (optional).
        /// </summary>
        /// <remarks>
        /// Used for user interfaces and reports. When null, the current value is preserved.
        /// </remarks>
        /// <value>
        /// String or null.
        /// </value>
        /// <example>
        /// مس
        /// </example>
        public string? Name { get; set; }

        /// <summary>
        /// Atomic mass of the element (optional).
        /// </summary>
        /// <remarks>
        /// Measured in unified atomic mass units (u). Should be non-negative and use appropriate precision per domain rules.
        /// </remarks>
        /// <value>
        /// Decimal value or null. Unit: u.
        /// </value>
        /// <example>
        /// 63.546
        /// </example>
        public decimal? AtomicMass { get; set; }

        /// <summary>
        /// Activation flag indicating whether the element is active (optional).
        /// </summary>
        /// <remarks>
        /// When true, the element is available for selection/processing. When false, it may be hidden or disabled.
        /// Null means "do not change".
        /// </remarks>
        /// <value>
        /// Boolean value or null.
        /// </value>
        /// <example>
        /// true
        /// </example>
        public bool? IsActive { get; set; }

        /// <summary>
        /// Display order for sorting elements in UI (optional).
        /// </summary>
        /// <remarks>
        /// Lower numbers typically appear earlier. Must be non-negative when provided. Null means "do not change".
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