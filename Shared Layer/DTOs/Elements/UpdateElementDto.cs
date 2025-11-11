namespace Shared.Icp.DTOs.Elements
{
    /// <summary>
    /// DTO برای ویرایش عنصر شیمیایی
    /// </summary>
    public class UpdateElementDto
    {
        /// <summary>
        /// نام فارسی عنصر - اختیاری
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// جرم اتمی - اختیاری
        /// </summary>
        public decimal? AtomicMass { get; set; }

        /// <summary>
        /// فعال/غیرفعال - اختیاری
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>
        /// ترتیب نمایش - اختیاری
        /// </summary>
        public int? DisplayOrder { get; set; }
    }
}