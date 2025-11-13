using System.ComponentModel.DataAnnotations;

namespace Shared.Icp.DTOs.CRM
{
    /// <summary>
    /// Data transfer object used to create a certified value for a CRM (Certified Reference Material).
    /// </summary>
    /// <remarks>
    /// Validation attributes include Persian user-facing error messages to ensure localized feedback.
    /// Numerical constraints (non-negative) are expressed via <see cref="RangeAttribute"/>.
    /// </remarks>
    public class CreateCRMValueDto
    {
        /// <summary>
        /// Identifier of the element this certified value belongs to. Required.
        /// </summary>
        [Required(ErrorMessage = "شناسه عنصر الزامی است")]
        public int ElementId { get; set; }

        /// <summary>
        /// The certified concentration/value for the element in the CRM. Required and must be non-negative.
        /// </summary>
        [Required(ErrorMessage = "مقدار تایید شده الزامی است")]
        [Range(0, double.MaxValue, ErrorMessage = "مقدار تایید شده نمی‌تواند منفی باشد")]
        public decimal CertifiedValue { get; set; }

        /// <summary>
        /// Optional measurement uncertainty associated with the certified value. Must be non-negative when provided.
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "عدم قطعیت نمی‌تواند منفی باشد")]
        public decimal? Uncertainty { get; set; }

        /// <summary>
        /// Optional lower acceptable limit for the certified value (same unit as <see cref="Unit"/>). Must be non-negative when provided.
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "حد پایین نمی‌تواند منفی باشد")]
        public decimal? LowerLimit { get; set; }

        /// <summary>
        /// Optional upper acceptable limit for the certified value (same unit as <see cref="Unit"/>). Must be non-negative when provided.
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "حد بالا نمی‌تواند منفی باشد")]
        public decimal? UpperLimit { get; set; }

        /// <summary>
        /// The unit of measurement for the certified value and limits. Required. Max length: 20. Default: ppm.
        /// </summary>
        [Required(ErrorMessage = "واحد الزامی است")]
        [StringLength(20, ErrorMessage = "واحد نباید بیشتر از 20 کاراکتر باشد")]
        public string Unit { get; set; } = "ppm";
    }
}