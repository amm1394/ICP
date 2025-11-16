using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Core.Icp.Domain.Entities.Projects;

namespace Core.Icp.Domain.Interfaces.Services
{
    /// <summary>
    /// سرویس دامنه برای مدیریت پروژه‌های آنالیز.
    /// </summary>
    public interface IProjectService
    {
        /// <summary>
        /// همه پروژه‌ها (برای گزارش‌ها و دیباگ).
        /// </summary>
        Task<IEnumerable<Project>> GetAllProjectsAsync();

        /// <summary>
        /// دریافت یک پروژه بر اساس Id (می‌تواند همراه Samples باشد).
        /// </summary>
        Task<Project?> GetProjectByIdAsync(Guid id);

        /// <summary>
        /// پروژه‌های اخیر (برای داشبورد و لیست‌های کوتاه).
        /// </summary>
        Task<IEnumerable<Project>> GetRecentProjectsAsync(int count);

        /// <summary>
        /// ایجاد پروژه جدید.
        /// </summary>
        Task<Project> CreateProjectAsync(Project project);

        /// <summary>
        /// به‌روزرسانی پروژه.
        /// </summary>
        Task<Project> UpdateProjectAsync(Project project);

        /// <summary>
        /// حذف نرم پروژه.
        /// </summary>
        Task<bool> DeleteProjectAsync(Guid id);

        /// <summary>
        /// ذخیره پروژه در فایل (در فازهای بعدی).
        /// </summary>
        Task<string> SaveProjectToFileAsync(Guid projectId, string filePath);

        /// <summary>
        /// بارگذاری پروژه از فایل (در فازهای بعدی).
        /// </summary>
        Task<Project> LoadProjectFromFileAsync(string filePath);

        /// <summary>
        /// دریافت تنظیمات پروژه به‌صورت ProjectSettings (از SettingsJson).
        /// اگر تنظیمات وجود نداشته باشد، مقدار پیش‌فرض برمی‌گرداند.
        /// </summary>
        Task<ProjectSettings?> GetProjectSettingsAsync(
            Guid projectId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// به‌روزرسانی تنظیمات پروژه (ProjectSettings) و ذخیره در دیتابیس.
        /// </summary>
        Task<ProjectSettings> UpdateProjectSettingsAsync(
            Guid projectId,
            ProjectSettings settings,
            CancellationToken cancellationToken = default);
    }
}
