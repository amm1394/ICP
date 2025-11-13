namespace Shared.Icp.DTOs.CRM
{
    /// <summary>
    /// Data transfer object representing a certified value for a specific element within a CRM.
    /// </summary>
    /// <remarks>
    /// This DTO is used to move certified composition data between layers. User-facing messages
    /// (e.g., validation or API responses) are localized in Persian elsewhere; this type only
    /// conveys data. Numerical values are typically reported in the specified <see cref="Unit"/>.
    /// </remarks>
    public class CRMValueDto
    {
        /// <summary>
        /// Unique identifier of this CRM value record.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Identifier of the parent CRM to which this value belongs.
        /// </summary>
        public int CRMId { get; set; }

        /// <summary>
        /// Identifier of the element associated with this certified value.
        /// </summary>
        public int ElementId { get; set; }

        /// <summary>
        /// Chemical element symbol (e.g., "Fe").
        /// </summary>
        public string ElementSymbol { get; set; } = string.Empty;

        /// <summary>
        /// Chemical element name (e.g., "Iron").
        /// </summary>
        public string ElementName { get; set; } = string.Empty;

        /// <summary>
        /// The certified concentration/value for the element in the CRM.
        /// </summary>
        public decimal CertifiedValue { get; set; }

        /// <summary>
        /// Optional measurement uncertainty associated with the certified value.
        /// </summary>
        public decimal? Uncertainty { get; set; }

        /// <summary>
        /// Optional lower acceptable limit for the certified value (same unit as <see cref="Unit"/>).
        /// </summary>
        public decimal? LowerLimit { get; set; }

        /// <summary>
        /// Optional upper acceptable limit for the certified value (same unit as <see cref="Unit"/>).
        /// </summary>
        public decimal? UpperLimit { get; set; }

        /// <summary>
        /// The unit of measurement for the certified value and limits. Defaults to parts per million (ppm).
        /// </summary>
        public string Unit { get; set; } = "ppm";
    }
}