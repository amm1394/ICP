// مسیر فایل: Src/Application/Services/QualityControl/QualityControlService.cs

using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces; // برای دسترسی به IUnitOfWork
using Domain.Interfaces.Services; // برای دسترسی به IQualityControlService
using Domain.Models; // برای ProjectSettings و ProjectQualitySummary

namespace Application.Services.QualityControl;

public class QualityControlService(IUnitOfWork unitOfWork) : IQualityControlService
{
    /// <summary>
    /// اجرای تمام چک‌های کنترل کیفیت به صورت ترتیبی
    /// </summary>
    public async Task<int> RunAllChecksAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        int totalFailures = 0;

        // ترتیب اجرای چک‌ها
        totalFailures += await RunCheckAsync(projectId, CheckType.WeightCheck, cancellationToken);
        totalFailures += await RunCheckAsync(projectId, CheckType.VolumeCheck, cancellationToken);
        totalFailures += await RunCheckAsync(projectId, CheckType.DilutionFactorCheck, cancellationToken);
        totalFailures += await RunCheckAsync(projectId, CheckType.EmptyCheck, cancellationToken);

        return totalFailures;
    }

    /// <summary>
    /// اجرای یک نوع چک خاص روی تمام نمونه‌های پروژه
    /// </summary>
    public async Task<int> RunCheckAsync(Guid projectId, CheckType checkType, CancellationToken cancellationToken = default)
    {
        // 1. دریافت پروژه برای خواندن تنظیمات
        var projectRepo = unitOfWork.Repository<Project>();
        var project = await projectRepo.GetByIdAsync(projectId);

        if (project == null) throw new Exception($"Project with ID {projectId} not found.");

        // خواندن تنظیمات از JSON ذخیره شده در پروژه
        var settings = project.GetSettings<ProjectSettings>() ?? new ProjectSettings();

        // اگر QC خودکار غیرفعال باشد، ادامه نده
        if (!settings.AutoQualityControl) return 0;

        // 2. دریافت نمونه‌ها به همراه اطلاعات لازم (QualityChecks و Measurements)
        var sampleRepo = unitOfWork.Repository<Sample>();
        var samples = await sampleRepo.GetAsync(
            s => s.ProjectId == projectId,
            includeProperties: "QualityChecks,Measurements"
        );

        var qualityCheckRepo = unitOfWork.Repository<QualityCheck>();
        int failedCount = 0;

        // 3. حلقه روی نمونه‌ها و بررسی قوانین
        foreach (var sample in samples)
        {
            // ارزیابی نمونه بر اساس نوع چک و تنظیمات
            var (status, message) = EvaluateSample(sample, checkType, settings);

            // پیدا کردن رکورد QC قبلی (اگر وجود داشته باشد) یا ساخت جدید
            var qcEntry = sample.QualityChecks.FirstOrDefault(q => q.CheckType == checkType);

            if (qcEntry == null)
            {
                qcEntry = new QualityCheck
                {
                    SampleId = sample.Id,
                    CheckType = checkType,
                    ProjectId = projectId,
                    CreatedAt = DateTime.UtcNow
                };
                await qualityCheckRepo.AddAsync(qcEntry);
            }

            // به‌روزرسانی وضعیت و پیام
            qcEntry.Status = status;
            qcEntry.Message = message;
            qcEntry.LastModified = DateTime.UtcNow;

            if (status == CheckStatus.Fail) failedCount++;
        }

        // 4. ذخیره تمام تغییرات در دیتابیس
        await unitOfWork.CommitAsync(cancellationToken);

        return failedCount;
    }

    /// <summary>
    /// دریافت خلاصه آماری وضعیت QC پروژه
    /// </summary>
    public async Task<ProjectQualitySummary> GetSummaryAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        var checkRepo = unitOfWork.Repository<QualityCheck>();
        var checks = await checkRepo.GetAsync(q => q.ProjectId == projectId);

        return new ProjectQualitySummary
        {
            ProjectId = projectId,
            TotalChecks = checks.Count,
            PassedCount = checks.Count(c => c.Status == CheckStatus.Pass),
            FailedCount = checks.Count(c => c.Status == CheckStatus.Fail),
            WarningCount = checks.Count(c => c.Status == CheckStatus.Warning)
        };
    }

    // =================================================================
    //  Logic Engine (موتور قوانین)
    // =================================================================

    private static (CheckStatus Status, string Message) EvaluateSample(Sample sample, CheckType type, ProjectSettings settings)
    {
        return type switch
        {
            CheckType.WeightCheck => ValidateRange(
                sample.Weight,
                settings.MinAcceptableWeight,
                settings.MaxAcceptableWeight,
                "Weight", "g"),

            CheckType.VolumeCheck => ValidateRange(
                sample.Volume,
                settings.MinAcceptableVolume,
                settings.MaxAcceptableVolume,
                "Volume", "mL"),

            CheckType.DilutionFactorCheck => ValidateRange(
                sample.DilutionFactor,
                settings.MinDilutionFactor,
                settings.MaxDilutionFactor,
                "DF", ""),

            CheckType.EmptyCheck => ValidateEmpty(sample),

            // برای انواع دیگر (مثل CRM) که منطق پیچیده‌تری دارند و جداگانه هندل می‌شوند
            _ => (CheckStatus.Pending, "Check logic not implemented in generic runner")
        };
    }

    private static (CheckStatus, string) ValidateRange(double value, double? min, double? max, string name, string unit)
    {
        // چک کردن مقادیر نامعتبر (صفر یا منفی)
        if (value <= 0)
            return (CheckStatus.Fail, $"{name} is invalid (<= 0).");

        // چک کردن حداقل
        if (min.HasValue && value < min.Value)
            return (CheckStatus.Fail, $"{name} ({value}{unit}) is below minimum ({min}{unit}).");

        // چک کردن حداکثر
        if (max.HasValue && value > max.Value)
            return (CheckStatus.Fail, $"{name} ({value}{unit}) is above maximum ({max}{unit}).");

        return (CheckStatus.Pass, "OK");
    }

    private static (CheckStatus, string) ValidateEmpty(Sample sample)
    {
        if (sample.Measurements == null || !sample.Measurements.Any())
            return (CheckStatus.Fail, "Sample has no measurements.");

        // اگر تمام مقادیر اندازه‌گیری شده صفر باشند، یعنی نمونه خالی یا خراب است
        bool hasValidData = sample.Measurements.Any(m => m.Value != 0);

        return hasValidData
            ? (CheckStatus.Pass, "OK")
            : (CheckStatus.Fail, "All measurements are zero.");
    }
}