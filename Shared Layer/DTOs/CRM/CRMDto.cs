using Shared.Icp.DTOs.Common;

namespace Shared.Icp.DTOs.CRM
{
    /// <summary>
    /// Data transfer object used to present a Certified Reference Material (CRM) and its certified values.
    /// </summary>
    /// <remarks>
    /// Inherits common metadata from <see cref="BaseDto"/> (e.g., <see cref="BaseDto.Id"/>, timestamps).
    /// This type is a pure data carrier; any user-facing messages (validation or API responses)
    /// are localized in Persian elsewhere in the application.
    /// </remarks>
    public class CRMDto : BaseDto
    {
        /// <summary>
        /// Human-readable CRM identifier/code provided by the supplier or system.
        /// </summary>
        public string CRMId { get; set; } = string.Empty;

        /// <summary>
        /// Display name of the CRM.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Manufacturer or supplier of the CRM. Optional.
        /// </summary>
        public string? Manufacturer { get; set; }

        /// <summary>
        /// Lot or batch number of the CRM. Optional.
        /// </summary>
        public string? LotNumber { get; set; }

        /// <summary>
        /// Expiration date of the CRM, if applicable. Optional.
        /// </summary>
        public DateTime? ExpirationDate { get; set; }

        /// <summary>
        /// Indicates whether the CRM is currently active/usable.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Free-form notes or remarks about the CRM. Optional.
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Number of certified values associated with this CRM.
        /// </summary>
        public int CertifiedValueCount { get; set; }

        /// <summary>
        /// Collection of certified values for specific elements within the CRM.
        /// </summary>
        public List<CRMValueDto> CertifiedValues { get; set; } = new();
    }
}