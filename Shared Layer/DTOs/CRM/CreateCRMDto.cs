using System.ComponentModel.DataAnnotations;

namespace Shared.Icp.DTOs.CRM
{
    /// <summary>
    /// Data transfer object used to create a new Certified Reference Material (CRM).
    /// </summary>
    /// <remarks>
    /// Validation attributes include Persian user-facing error messages to ensure localized feedback.
    /// Length limits reflect application-wide constraints for CRM metadata.
    /// </remarks>
    public class CreateCRMDto
    {
        /// <summary>
        /// Human-readable CRM identifier/code. Required. Maximum length: 50 characters.
        /// </summary>
        [Required(ErrorMessage = "شناسه CRM الزامی است")]
        [StringLength(50, ErrorMessage = "شناسه CRM نباید بیشتر از 50 کاراکتر باشد")]
        public string CRMId { get; set; } = string.Empty;

        /// <summary>
        /// Display name of the CRM. Required. Maximum length: 200 characters.
        /// </summary>
        [Required(ErrorMessage = "نام CRM الزامی است")]
        [StringLength(200, ErrorMessage = "نام CRM نباید بیشتر از 200 کاراکتر باشد")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Manufacturer or supplier of the CRM. Optional. Maximum length: 100 characters.
        /// </summary>
        [StringLength(100, ErrorMessage = "نام تولیدکننده نباید بیشتر از 100 کاراکتر باشد")]
        public string? Manufacturer { get; set; }

        /// <summary>
        /// Lot or batch number of the CRM. Optional. Maximum length: 50 characters.
        /// </summary>
        [StringLength(50, ErrorMessage = "شماره سری نباید بیشتر از 50 کاراکتر باشد")]
        public string? LotNumber { get; set; }

        /// <summary>
        /// Expiration date of the CRM, if applicable. Optional.
        /// </summary>
        public DateTime? ExpirationDate { get; set; }

        /// <summary>
        /// Optional free-form notes about the CRM. Maximum length: 1000 characters.
        /// </summary>
        [StringLength(1000, ErrorMessage = "توضیحات نباید بیشتر از 1000 کاراکتر باشد")]
        public string? Notes { get; set; }

        /// <summary>
        /// Certified element values to be associated with this CRM at creation time. Optional collection.
        /// </summary>
        public List<CreateCRMValueDto> CertifiedValues { get; set; } = new();
    }
}