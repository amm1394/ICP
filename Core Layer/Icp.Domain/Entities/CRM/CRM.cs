using Core.Icp.Domain.Base;

namespace Core.Icp.Domain.Entities.CRM
{
    /// <summary>
    /// Represents a Certified Reference Material (CRM), a traceable standard used for
    /// instrument calibration, method validation, and ongoing quality control (QC).
    /// </summary>
    /// <remarks>
    /// A CRM contains metadata (identifier, name, manufacturer, lot number, etc.) and a set of
    /// certified element values with their uncertainties. These values are consumed when building
    /// calibration curves and during QC checks (e.g., verification and drift control).
    /// </remarks>
    public class CRM : BaseEntity
    {
        /// <summary>
        /// Gets or sets the custom identifier for the CRM (e.g., an internal or supplier code).
        /// This value is typically unique and is used for quick lookup and display in UIs.
        /// </summary>
        public string CRMId { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the official name of the Certified Reference Material
        /// (e.g., "NIST SRM 1643f - Trace Elements in Water").
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the manufacturer that produced the CRM
        /// (e.g., "NIST", "Inorganic Ventures"). Optional.
        /// </summary>
        public string? Manufacturer { get; set; }

        /// <summary>
        /// Gets or sets the lot number identifying a specific production batch of the CRM.
        /// Useful for traceability and audit purposes. Optional.
        /// </summary>
        public string? LotNumber { get; set; }

        /// <summary>
        /// Gets or sets the expiration date of the CRM certification (UTC).
        /// A null value indicates no specified expiration or it is unknown.
        /// </summary>
        public DateTime? ExpirationDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the CRM is currently active and selectable
        /// in the application. Deactivating a CRM does not delete it and preserves history.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets additional notes or comments relevant to the CRM, such as storage
        /// conditions, preparation instructions, or certificate references.
        /// </summary>
        public string? Notes { get; set; }

        // Navigation Properties
        /// <summary>
        /// Gets or sets the collection of certified values for elements contained in this CRM.
        /// Each item provides the certified concentration, unit, and (optionally) uncertainty
        /// for a specific element. This is an EF navigation property.
        /// </summary>
        public ICollection<CRMValue> CertifiedValues { get; set; } = new List<CRMValue>();
    }
}