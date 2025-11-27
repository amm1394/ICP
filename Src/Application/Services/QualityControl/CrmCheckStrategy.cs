using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces.Services;
using Domain.Models;

namespace Application.Services.QualityControl.Strategies;

public class CrmCheckStrategy(ICrmService crmService) : IQualityCheckStrategy
{
    public CheckType CheckType => CheckType.CRMCheck;

    public async Task<(List<Guid>, string)> ExecuteAsync(List<Sample> samples, ProjectSettings settings, CancellationToken cancellationToken)
    {
        var failedIds = new List<Guid>();

        // 1. فقط نمونه‌های استاندارد (STD) را بررسی می‌کنیم
        var standards = samples.Where(s => s.Type == SampleType.Standard).ToList();

        if (!standards.Any())
            return (failedIds, "No Standard/CRM samples found.");

        // تنظیمات درصد مجاز (پیش‌فرض ۱۰٪ خطا)
        double minRecovery = settings.MinRecoveryPercentage ?? 90.0;
        double maxRecovery = settings.MaxRecoveryPercentage ?? 110.0;

        foreach (var std in standards)
        {
            // 2. دریافت مقادیر واقعی (Certified Values) این استاندارد از دیتابیس
            // فرض: نام استاندارد در SolutionLabel نوشته شده (مثلاً "OREAS 123")
            var certifiedValues = await crmService.GetCertifiedValuesAsync(std.SolutionLabel, cancellationToken);

            if (certifiedValues == null || !certifiedValues.Any())
            {
                // اگر استاندارد در سیستم تعریف نشده باشد، فعلاً نادیده می‌گیریم (یا می‌توان هشدار داد)
                continue;
            }

            bool sampleFailed = false;

            // 3. مقایسه هر عنصر
            foreach (var measurement in std.Measurements)
            {
                if (certifiedValues.TryGetValue(measurement.ElementName, out double expectedValue))
                {
                    if (expectedValue == 0) continue;

                    // محاسبه درصد بازیابی: (مقدار خوانده شده / مقدار واقعی) * ۱۰۰
                    double recovery = (measurement.Value / expectedValue) * 100.0;

                    if (recovery < minRecovery || recovery > maxRecovery)
                    {
                        sampleFailed = true;
                        // می‌توانیم جزئیات خطا را هم لاگ کنیم
                        break; // اگر حتی یک عنصر خطا داشت، کل نمونه فیل است
                    }
                }
            }

            if (sampleFailed)
            {
                failedIds.Add(std.Id);
            }
        }

        return (failedIds, $"CRM Recovery out of range ({minRecovery}%-{maxRecovery}%).");
    }
}