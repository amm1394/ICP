using Core.Icp.Application.Models.CRM;

namespace Core.Icp.Application.Interfaces
{
    public interface ICRMService
    {
        /// <summary>
        /// لیست CRMها (با امکان فیلتر فقط فعال‌ها)
        /// </summary>
        Task<IReadOnlyCollection<CRMDto>> GetCrmsAsync(
            bool onlyActive = true,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// گرفتن یک CRM با Id به همراه مقادیر عناصر
        /// </summary>
        Task<CRMDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// جستجو بر اساس Code یا Matrix
        /// </summary>
        Task<IReadOnlyCollection<CRMDto>> SearchAsync(
            string? code = null,
            string? matrix = null,
            bool onlyActive = true,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// ایجاد CRM جدید به همراه مقادیر عناصر
        /// </summary>
        Task<CRMDto> CreateAsync(CreateCRMDto dto, CancellationToken cancellationToken = default);

        /// <summary>
        /// ویرایش CRM و مقادیر عناصر
        /// </summary>
        Task<CRMDto> UpdateAsync(UpdateCRMDto dto, CancellationToken cancellationToken = default);

        /// <summary>
        /// حذف نرم (Soft Delete)
        /// </summary>
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
