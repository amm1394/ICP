using System.Collections.Generic;
using System.Linq;
using Core.Icp.Domain.Entities.Projects;
using Core.Icp.Domain.Models.QualityControl;
using Shared.Icp.DTOs.QualityControl;

namespace Shared.Icp.Helpers.Mappers
{
    /// <summary>
    /// متدهای کمکی برای تبدیل مدل‌های QC دامنه به DTOهای API.
    /// </summary>
    public static class QualityControlMapper
    {
        public static QualityCheckResultDto ToDto(this QualityCheckResult result)
        {
            return new QualityCheckResultDto
            {
                ProjectId = result.ProjectId,
                SampleId = result.SampleId,
                CheckType = result.CheckType,
                Status = result.Status,
                Message = result.Message
            };
        }

        public static List<QualityCheckResultDto> ToDtoList(this IEnumerable<QualityCheckResult> results)
        {
            return results.Select(r => r.ToDto()).ToList();
        }

        public static ProjectQualitySummaryDto ToDto(this ProjectQualitySummary summary)
        {
            return new ProjectQualitySummaryDto
            {
                ProjectId = summary.ProjectId,
                TotalSamples = summary.TotalSamples,
                TotalChecks = summary.TotalChecks,
                PassedCount = summary.PassedCount,
                WarningCount = summary.WarningCount,
                FailedCount = summary.FailedCount,
                NotImplementedCount = summary.NotImplementedCount
            };
        }

        /// <summary>
        /// تبدیل ProjectSettings به DTO تنظیمات QC.
        /// </summary>
        public static ProjectQualitySettingsDto ToQualitySettingsDto(
            this ProjectSettings settings,
            Guid projectId)
        {
            return new ProjectQualitySettingsDto
            {
                ProjectId = projectId,
                AutoQualityControl = settings.AutoQualityControl,
                MinAcceptableWeight = settings.MinAcceptableWeight,
                MaxAcceptableWeight = settings.MaxAcceptableWeight,
                MinAcceptableVolume = settings.MinAcceptableVolume,
                MaxAcceptableVolume = settings.MaxAcceptableVolume,
                MinDilutionFactor = settings.MinDilutionFactor,
                MaxDilutionFactor = settings.MaxDilutionFactor
            };
        }

        /// <summary>
        /// اعمال مقادیر DTO روی ProjectSettings موجود (یا ساخت جدید).
        /// </summary>
        public static ProjectSettings ApplyFromDto(
            this ProjectSettings target,
            ProjectQualitySettingsDto dto)
        {
            target.AutoQualityControl = dto.AutoQualityControl;
            target.MinAcceptableWeight = dto.MinAcceptableWeight;
            target.MaxAcceptableWeight = dto.MaxAcceptableWeight;
            target.MinAcceptableVolume = dto.MinAcceptableVolume;
            target.MaxAcceptableVolume = dto.MaxAcceptableVolume;
            target.MinDilutionFactor = dto.MinDilutionFactor;
            target.MaxDilutionFactor = dto.MaxDilutionFactor;

            return target;
        }
    }
}
