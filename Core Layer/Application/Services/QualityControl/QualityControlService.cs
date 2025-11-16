using Core.Icp.Domain.Entities.Projects;
using Core.Icp.Domain.Entities.QualityControl;
using Core.Icp.Domain.Entities.Samples;
using Core.Icp.Domain.Enums;
using Core.Icp.Domain.Interfaces.Repositories;
using Core.Icp.Domain.Interfaces.Services;
using Core.Icp.Domain.Models.QualityControl;

namespace Core.Icp.Application.Services.QualityControl
{
    /// <summary>
    /// پیاده‌سازی سرویس کنترل کیفیت (QC) بر اساس تنظیمات پروژه و اطلاعات نمونه.
    /// </summary>
    public class QualityControlService : IQualityControlService
    {
        private readonly IUnitOfWork _unitOfWork;

        public QualityControlService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        #region Helpers

        private async Task<Project> EnsureProjectLoadedAsync(Sample sample)
        {
            if (sample.Project != null)
                return sample.Project;

            var project = await _unitOfWork.Projects.GetByIdAsync(sample.ProjectId);
            if (project == null)
                throw new InvalidOperationException("پروژه‌ی مربوط به نمونه یافت نشد.");

            sample.Project = project;
            return project;
        }

        private static ProjectSettings GetProjectSettingsOrDefault(Project project)
        {
            // SettingsJson -> ProjectSettings
            var settings = project.GetSettings<ProjectSettings>();
            return settings ?? new ProjectSettings();
        }

        private static QualityCheckResult MapToResult(Project project, Sample sample, QualityCheck check)
        {
            return new QualityCheckResult
            {
                ProjectId = project.Id,
                SampleId = sample.Id,
                CheckType = check.CheckType.ToString(),
                Status = check.Status.ToString(),
                Message = check.Message ?? string.Empty
            };
        }

        #endregion

        #region Weight Check (per sample)

        /// <summary>
        /// اجرای کنترل وزن برای یک نمونه و ذخیره نتیجه در دیتابیس.
        /// </summary>
        public async Task<QualityCheck> PerformWeightCheckAsync(Sample sample)
        {
            if (sample == null) throw new ArgumentNullException(nameof(sample));

            var project = await EnsureProjectLoadedAsync(sample);
            var settings = GetProjectSettingsOrDefault(project);

            var check = new QualityCheck
            {
                CheckType = CheckType.WeightCheck,
                SampleId = sample.Id,
                Sample = sample
            };

            var weight = (double)sample.Weight;

            if (!settings.AutoQualityControl)
            {
                check.Status = CheckStatus.Warning;
                check.Message = "کنترل کیفیت خودکار برای این پروژه غیرفعال است.";
                check.Details = $"وزن نمونه: {weight} g";
            }
            else if (!settings.MinAcceptableWeight.HasValue && !settings.MaxAcceptableWeight.HasValue)
            {
                check.Status = CheckStatus.Warning;
                check.Message = "حداقل و حداکثر وزن مجاز در تنظیمات پروژه تعریف نشده است.";
                check.Details = $"وزن نمونه: {weight} g";
            }
            else if (weight <= 0)
            {
                check.Status = CheckStatus.Fail;
                check.Message = "وزن نمونه نامعتبر است (صفر یا منفی).";
                check.Details = $"وزن نمونه: {weight} g";
            }
            else
            {
                var min = settings.MinAcceptableWeight;
                var max = settings.MaxAcceptableWeight;

                var below = min.HasValue && weight < min.Value;
                var above = max.HasValue && weight > max.Value;

                if (below || above)
                {
                    check.Status = CheckStatus.Fail;
                    check.Message = "وزن نمونه خارج از محدوده مجاز است.";
                }
                else
                {
                    check.Status = CheckStatus.Pass;
                    check.Message = "وزن نمونه در محدوده مجاز قرار دارد.";
                }

                check.Details =
                    $"وزن: {weight:0.###} g؛ حداقل مجاز: {(min.HasValue ? min.Value.ToString("0.###") : "-")} g؛ حداکثر مجاز: {(max.HasValue ? max.Value.ToString("0.###") : "-")} g";
            }

            await _unitOfWork.QualityChecks.AddAsync(check);
            await _unitOfWork.SaveChangesAsync();

            return check;
        }


        #endregion

        #region Volume Check (per sample)
        public async Task<QualityCheck> PerformVolumeCheckAsync(Sample sample)
        {
            if (sample == null) throw new ArgumentNullException(nameof(sample));

            var project = await EnsureProjectLoadedAsync(sample);
            var settings = GetProjectSettingsOrDefault(project);

            var check = new QualityCheck
            {
                CheckType = CheckType.VolumeCheck,
                SampleId = sample.Id,
                Sample = sample
            };

            var volume = (double)sample.Volume;

            if (!settings.AutoQualityControl)
            {
                check.Status = CheckStatus.Warning;
                check.Message = "کنترل کیفیت خودکار برای این پروژه غیرفعال است.";
                check.Details = $"حجم نمونه: {volume} mL";
            }
            else if (!settings.MinAcceptableVolume.HasValue && !settings.MaxAcceptableVolume.HasValue)
            {
                check.Status = CheckStatus.Warning;
                check.Message = "حداقل و حداکثر حجم مجاز در تنظیمات پروژه تعریف نشده است.";
                check.Details = $"حجم نمونه: {volume} mL";
            }
            else if (volume <= 0)
            {
                check.Status = CheckStatus.Fail;
                check.Message = "حجم نمونه نامعتبر است (صفر یا منفی).";
                check.Details = $"حجم نمونه: {volume} mL";
            }
            else
            {
                var min = settings.MinAcceptableVolume;
                var max = settings.MaxAcceptableVolume;

                var below = min.HasValue && volume < min.Value;
                var above = max.HasValue && volume > max.Value;

                if (below || above)
                {
                    check.Status = CheckStatus.Fail;
                    check.Message = "حجم نمونه خارج از محدوده مجاز است.";
                }
                else
                {
                    check.Status = CheckStatus.Pass;
                    check.Message = "حجم نمونه در محدوده مجاز قرار دارد.";
                }

                check.Details =
                    $"حجم: {volume:0.###} mL؛ حداقل مجاز: {(min.HasValue ? min.Value.ToString("0.###") : "-")} mL؛ حداکثر مجاز: {(max.HasValue ? max.Value.ToString("0.###") : "-")} mL";
            }

            await _unitOfWork.QualityChecks.AddAsync(check);
            await _unitOfWork.SaveChangesAsync();

            return check;
        }
        #endregion

        #region Dilution Factor Check (per sample)

        public async Task<QualityCheck> PerformDilutionFactorCheckAsync(Sample sample)
        {
            if (sample == null) throw new ArgumentNullException(nameof(sample));

            var project = await EnsureProjectLoadedAsync(sample);
            var settings = GetProjectSettingsOrDefault(project);

            var check = new QualityCheck
            {
                CheckType = CheckType.DilutionFactorCheck,
                SampleId = sample.Id,
                Sample = sample
            };

            var df = (int)sample.DilutionFactor;

            if (!settings.AutoQualityControl)
            {
                check.Status = CheckStatus.Warning;
                check.Message = "کنترل کیفیت خودکار برای این پروژه غیرفعال است.";
                check.Details = $"فاکتور رقت: {df}";
            }
            else if (!settings.MinDilutionFactor.HasValue && !settings.MaxDilutionFactor.HasValue)
            {
                check.Status = CheckStatus.Warning;
                check.Message = "محدوده مجاز فاکتور رقت در تنظیمات پروژه تعریف نشده است.";
                check.Details = $"فاکتور رقت: {df}";
            }
            else if (df <= 0)
            {
                check.Status = CheckStatus.Fail;
                check.Message = "فاکتور رقت نامعتبر است (صفر یا منفی).";
                check.Details = $"فاکتور رقت: {df}";
            }
            else
            {
                var min = settings.MinDilutionFactor;
                var max = settings.MaxDilutionFactor;

                var below = min.HasValue && df < min.Value;
                var above = max.HasValue && df > max.Value;

                if (below || above)
                {
                    check.Status = CheckStatus.Fail;
                    check.Message = "فاکتور رقت خارج از محدوده مجاز است.";
                }
                else
                {
                    check.Status = CheckStatus.Pass;
                    check.Message = "فاکتور رقت در محدوده مجاز است.";
                }

                check.Details =
                    $"فاکتور رقت: {df}; حداقل مجاز: {(min.HasValue ? min.Value.ToString() : "-")}; حداکثر مجاز: {(max.HasValue ? max.Value.ToString() : "-")}";
            }

            await _unitOfWork.QualityChecks.AddAsync(check);
            await _unitOfWork.SaveChangesAsync();

            return check;
        }

        #endregion

        #region Empty Check (per sample)

        public async Task<QualityCheck> PerformEmptyCheckAsync(Sample sample)
        {
            if (sample == null) throw new ArgumentNullException(nameof(sample));

            var project = await EnsureProjectLoadedAsync(sample);
            _ = GetProjectSettingsOrDefault(project); // فعلاً تنظیم خاصی نیاز نداریم

            var check = new QualityCheck
            {
                CheckType = CheckType.EmptyCheck,
                SampleId = sample.Id,
                Sample = sample
            };

            var hasMeasurements = sample.Measurements != null && sample.Measurements.Any();

            if (!hasMeasurements && sample.Weight <= 0 && sample.Volume <= 0)
            {
                check.Status = CheckStatus.Fail;
                check.Message = "نمونه فاقد داده معتبر (وزن/حجم/اندازه‌گیری) است.";
                check.Details = "وزن و حجم صفر هستند و هیچ Measurementای ثبت نشده است.";
            }
            else
            {
                check.Status = CheckStatus.Pass;
                check.Message = "نمونه دارای داده‌ی پایه است.";
                check.Details =
                    $"وزن: {sample.Weight}; حجم: {sample.Volume}; تعداد اندازه‌گیری‌ها: {(sample.Measurements?.Count ?? 0)}";
            }

            await _unitOfWork.QualityChecks.AddAsync(check);
            await _unitOfWork.SaveChangesAsync();

            return check;
        }

        #endregion

        #region CRM & Drift (Placeholder)

        public Task<QualityCheck> PerformCRMCheckAsync(Sample sample, int crmId)
        {
            if (sample == null) throw new ArgumentNullException(nameof(sample));

            var check = new QualityCheck
            {
                CheckType = CheckType.CRMCheck,
                SampleId = sample.Id,
                Sample = sample,
                Status = CheckStatus.Pending,
                Message = "منطق کنترل CRM هنوز پیاده‌سازی نشده است.",
                Details = $"crmId = {crmId}"
            };

            // فعلاً در دیتابیس ذخیره نمی‌کنیم تا از آلودگی داده جلوگیری شود.
            return Task.FromResult(check);
        }

        public Task<QualityCheck> PerformDriftCalibrationAsync(Sample sample)
        {
            if (sample == null) throw new ArgumentNullException(nameof(sample));

            var check = new QualityCheck
            {
                CheckType = CheckType.DriftCalibration,
                SampleId = sample.Id,
                Sample = sample,
                Status = CheckStatus.Pending,
                Message = "منطق کنترل Drift هنوز پیاده‌سازی نشده است.",
                Details = "در فازهای بعدی بر اساس کدهای پایتون و قواعد آزمایشگاه تکمیل می‌شود."
            };

            // فعلاً در دیتابیس ذخیره نمی‌کنیم.
            return Task.FromResult(check);
        }

        #endregion

        #region IQualityControlService members (per-project)

        public async Task<IReadOnlyList<QualityCheckResult>> RunWeightChecksAsync(
            Guid projectId,
            CancellationToken cancellationToken = default)
        {
            var project = await _unitOfWork.Projects.GetWithSamplesAsync(projectId);
            if (project == null || project.Samples == null || project.Samples.Count == 0)
            {
                return Array.Empty<QualityCheckResult>();
            }

            var results = new List<QualityCheckResult>();

            foreach (var sample in project.Samples)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var check = await PerformWeightCheckAsync(sample);

                results.Add(MapToResult(project, sample, check));
            }

            return results;
        }

        public async Task<IReadOnlyList<QualityCheckResult>> RunVolumeChecksAsync(
            Guid projectId,
            CancellationToken cancellationToken = default)
        {
            var project = await _unitOfWork.Projects.GetWithSamplesAsync(projectId);
            if (project == null || project.Samples == null || project.Samples.Count == 0)
            {
                return Array.Empty<QualityCheckResult>();
            }

            var results = new List<QualityCheckResult>();

            foreach (var sample in project.Samples)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var check = await PerformVolumeCheckAsync(sample);

                results.Add(MapToResult(project, sample, check));
            }

            return results;
        }

        public async Task<IReadOnlyList<QualityCheckResult>> RunDilutionFactorChecksAsync(
            Guid projectId,
            CancellationToken cancellationToken = default)
        {
            var project = await _unitOfWork.Projects.GetWithSamplesAsync(projectId);
            if (project == null || project.Samples == null || project.Samples.Count == 0)
            {
                return Array.Empty<QualityCheckResult>();
            }

            var results = new List<QualityCheckResult>();

            foreach (var sample in project.Samples)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var check = await PerformDilutionFactorCheckAsync(sample);

                results.Add(MapToResult(project, sample, check));
            }

            return results;
        }

        public async Task<IReadOnlyList<QualityCheckResult>> RunEmptyChecksAsync(
            Guid projectId,
            CancellationToken cancellationToken = default)
        {
            var project = await _unitOfWork.Projects.GetWithSamplesAsync(projectId);
            if (project == null || project.Samples == null || project.Samples.Count == 0)
            {
                return Array.Empty<QualityCheckResult>();
            }

            var results = new List<QualityCheckResult>();

            foreach (var sample in project.Samples)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var check = await PerformEmptyCheckAsync(sample);

                results.Add(MapToResult(project, sample, check));
            }

            return results;
        }

        public async Task<IReadOnlyList<QualityCheckResult>> RunAllChecksAsync(
            Guid projectId,
            CancellationToken cancellationToken = default)
        {
            var allResults = new List<QualityCheckResult>();

            var weight = await RunWeightChecksAsync(projectId, cancellationToken);
            allResults.AddRange(weight);

            var volume = await RunVolumeChecksAsync(projectId, cancellationToken);
            allResults.AddRange(volume);

            var df = await RunDilutionFactorChecksAsync(projectId, cancellationToken);
            allResults.AddRange(df);

            var empty = await RunEmptyChecksAsync(projectId, cancellationToken);
            allResults.AddRange(empty);

            return allResults;
        }

        public async Task<ProjectQualitySummary> GetProjectQualitySummaryAsync(
            Guid projectId,
            CancellationToken cancellationToken = default)
        {
            var allResults = await RunAllChecksAsync(projectId, cancellationToken);

            var summary = new ProjectQualitySummary
            {
                ProjectId = projectId,
                TotalSamples = allResults
                    .Select(r => r.SampleId)
                    .Distinct()
                    .Count(),
                TotalChecks = allResults.Count,
                PassedCount = allResults.Count(r =>
                    r.Status.Equals("Pass", StringComparison.OrdinalIgnoreCase)),
                WarningCount = allResults.Count(r =>
                    r.Status.Equals("Warning", StringComparison.OrdinalIgnoreCase)),
                FailedCount = allResults.Count(r =>
                    r.Status.Equals("Fail", StringComparison.OrdinalIgnoreCase)),
                NotImplementedCount = allResults.Count(r =>
                    r.Status.Equals("NotImplemented", StringComparison.OrdinalIgnoreCase) ||
                    r.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase))
            };

            return summary;
        }

        #endregion
    }
}
