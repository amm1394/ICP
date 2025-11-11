namespace Shared.Icp.DTOs.Elements
{
    /// <summary>
    /// DTO برای ایجاد عنصر شیمیایی جدید
    /// </summary>
    public class CreateElementDto
    {
        /// <summary>
        /// نماد عنصر (مثل: Ce, La, Nd)
        /// </summary>
        public string Symbol { get; set; } = string.Empty;

        /// <summary>
        /// نام فارسی عنصر
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// عدد اتمی
        /// </summary>
        public int AtomicNumber { get; set; }

        /// <summary>
        /// جرم اتمی
        /// </summary>
        public decimal AtomicMass { get; set; }

        /// <summary>
        /// ترتیب نمایش (اختیاری)
        /// </summary>
        public int? DisplayOrder { get; set; }
    }
}