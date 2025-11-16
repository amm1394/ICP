using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Core.Icp.Domain.Models.QualityControl;

namespace Core.Icp.Domain.Interfaces.Services
{
    /// <summary>
    /// سرویس کنترل کیفیت (QC) برای اجرای انواع چک روی پروژه‌ها.
    /// </summary>
    public interface IQualityControlService
    {
        /// <summary>
        /// اجرای Weight QC برای همه نمونه‌های یک پروژه.
        /// </summary>
        Task<IReadOnlyList<QualityCheckResult>> RunWeightChecksAsync(
            Guid projectId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// اجرای Volume QC برای همه نمونه‌های یک پروژه.
        /// </summary>
        Task<IReadOnlyList<QualityCheckResult>> RunVolumeChecksAsync(
            Guid projectId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// اجرای Dilution Factor QC برای همه نمونه‌های یک پروژه.
        /// </summary>
        Task<IReadOnlyList<QualityCheckResult>> RunDilutionFactorChecksAsync(
            Guid projectId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// اجرای Empty QC برای همه نمونه‌های یک پروژه.
        /// </summary>
        Task<IReadOnlyList<QualityCheckResult>> RunEmptyChecksAsync(
            Guid projectId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// اجرای همه چک‌های پایه (Weight, Volume, Dilution, Empty) برای پروژه.
        /// </summary>
        Task<IReadOnlyList<QualityCheckResult>> RunAllChecksAsync(
            Guid projectId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// دریافت خلاصه نتایج QC برای یک پروژه (بر اساس همه چک‌های پایه).
        /// </summary>
        Task<ProjectQualitySummary> GetProjectQualitySummaryAsync(
            Guid projectId,
            CancellationToken cancellationToken = default);
    }
}
