using System.ComponentModel.DataAnnotations;

namespace Shared.Icp.DTOs.CRM
{
    /// <summary>
    /// Data transfer object used to update an existing Certified Reference Material (CRM).
    /// </summary>
    /// <remarks>
    /// Validation attributes include Persian user-facing error messages to ensure localized feedback.
    /// Length constraints reflect typical CRM metadata limits used throughout the application.
    /// </remarks>
    public class UpdateCRMDto
    {
        /// <summary>
        /// The unique identifier of the CRM to update. Required.
        /// </summary>
        [Required(ErrorMessage = "شناسه الزامی است")]
        public int Id { get; set; }

        /// <summary>
        /// The CRM display name. Required. Maximum length: 200 characters.
        /// </summary>
        [Required(ErrorMessage = "نام CRM الزامی است")]
        [StringLength(200, ErrorMessage = "نام CRM نباید بیشتر از 200 کاراکتر باشد")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The CRM manufacturer name. Optional. Maximum length: 100 characters.
        /// </summary>
        [StringLength(100, ErrorMessage = "نام تولیدکننده نباید بیشتر از 100 کاراکتر باشد")]
        public string? Manufacturer { get; set; }

        /// <summary>
        /// The CRM lot or batch number. Optional. Maximum length: 50 characters.
        /// </summary>
        [StringLength(50, ErrorMessage = "شماره سری نباید بیشتر از 50 کاراکتر باشد")]
        public string? LotNumber { get; set; }

        /// <summary>
        /// The CRM expiration date, if applicable. Optional.
        /// </summary>
        public DateTime? ExpirationDate { get; set; }

        /// <summary>
        /// Indicates whether the CRM is currently active/usable.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Optional free-form notes about the CRM. Maximum length: 1000 characters.
        /// </summary>
        [StringLength(1000, ErrorMessage = "توضیحات نباید بیشتر از 1000 کاراکتر باشد")]
        public string? Notes { get; set; }
    }
}