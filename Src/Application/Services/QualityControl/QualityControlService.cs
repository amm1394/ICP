using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Domain.Interfaces.Services;
using Domain.Models;
using System.Text.Json; // برای خواندن تنظیمات

namespace Application.Services.QualityControl;

public class QualityControlService(
    IUnitOfWork unitOfWork,
    IEnumerable<IQualityCheckStrategy> strategies // تزریق خودکار لیست تمام استراتژی‌ها
    ) : IQualityControlService
{
    public async Task<int> RunAllChecksAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        int totalFailures = 0;

        // ترتیب اجرای چک‌ها (منطق پایتون: اول چک‌های فیزیکی، بعد شیمیایی)
        var checkOrder = new[]
        {
            CheckType.WeightCheck,
            CheckType.VolumeCheck,
            CheckType.DilutionFactorCheck,
            CheckType.EmptyCheck,
            CheckType.CRMCheck // اگر استراتژی CRM اضافه شود، خودکار اجرا می‌شود
        };

        foreach (var check in checkOrder)
        {
            // اگر استراتژی مربوطه وجود نداشت، نادیده بگیر (برای جلوگیری از خطا در حین توسعه)
            if (strategies.Any(s => s.CheckType == check))
            {
                totalFailures += await RunCheckAsync(projectId, check, cancellationToken);
            }
        }

        return totalFailures;
    }

    public async Task<int> RunCheckAsync(Guid projectId, CheckType checkType, CancellationToken cancellationToken = default)
    {
        // 1. پیدا کردن استراتژی مناسب
        var strategy = strategies.FirstOrDefault(s => s.CheckType == checkType);
        if (strategy == null)
            throw new NotImplementedException($"No strategy implementation found for check type: {checkType}");

        // 2. دریافت پروژه برای خواندن تنظیمات
        var project = await unitOfWork.Repository<Project>().GetByIdAsync(projectId);
        if (project == null)
            throw new KeyNotFoundException($"Project with ID {projectId} not found.");

        // 3. لود کردن تنظیمات پروژه (اصلاح مهم: خواندن از JSON)
        ProjectSettings settings;
        try
        {
            settings = string.IsNullOrEmpty(project.SettingsJson)
                ? new ProjectSettings() // تنظیمات پیش‌فرض اگر JSON خالی بود
                : JsonSerializer.Deserialize<ProjectSettings>(project.SettingsJson) ?? new ProjectSettings();
        }
        catch
        {
            settings = new ProjectSettings(); // در صورت خطای فرمت جیسون، پیش‌فرض استفاده شود
        }

        // 4. دریافت نمونه‌ها به همراه داده‌های اندازه‌گیری (اصلاح مهم: Include Measurements)
        // برای چک‌هایی مثل EmptyCheck یا CRM، نیاز به مقادیر Measurements داریم
        var samples = (await unitOfWork.Repository<Sample>()
            .GetAsync(s => s.ProjectId == projectId, includeProperties: "Measurements")).ToList();

        if (!samples.Any())
            return 0;

        // 5. اجرای استراتژی و گرفتن لیست نمونه‌های مردود
        var (failedSampleIds, message) = await strategy.ExecuteAsync(samples, settings, cancellationToken);

        // 6. ذخیره نتایج در دیتابیس
        var qcRepo = unitOfWork.Repository<QualityCheck>();

        // ابتدا تمام چک‌های قبلی از این نوع برای این پروژه را پاک می‌کنیم (یا آپدیت می‌کنیم)
        // استراتژی Clean & Insert معمولاً برای QC ساده‌تر و مطمئن‌تر است
        var existingChecks = await qcRepo.GetAsync(q => q.ProjectId == projectId && q.CheckType == checkType);
        foreach (var check in existingChecks)
        {
            await qcRepo.DeleteAsync(check);
        }

        int newFailures = 0;
        foreach (var sample in samples)
        {
            // اگر در لیست مردودی‌ها بود -> Fail، وگرنه -> Pass
            var status = failedSampleIds.Contains(sample.Id) ? CheckStatus.Fail : CheckStatus.Pass;
            var resultMessage = status == CheckStatus.Fail ? message : "Passed";

            await qcRepo.AddAsync(new QualityCheck
            {
                ProjectId = projectId,
                SampleId = sample.Id,
                CheckType = checkType,
                Status = status,
                Message = resultMessage,
                CreatedAt = DateTime.UtcNow
            });

            if (status == CheckStatus.Fail) newFailures++;
        }

        await unitOfWork.CommitAsync(cancellationToken);
        return newFailures;
    }

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
}