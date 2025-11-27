namespace Domain.Models;

public class ProjectSettings
{
    // QC - Weight
    public double? MinAcceptableWeight { get; set; } = 0.190;
    public double? MaxAcceptableWeight { get; set; } = 0.210;

    // QC - Volume
    public double? MinAcceptableVolume { get; set; } = 48.0;
    public double? MaxAcceptableVolume { get; set; } = 52.0;

    // QC - Dilution Factor
    public double? MinDilutionFactor { get; set; } = 0.9;
    public double? MaxDilutionFactor { get; set; } = 1000.0;

    // QC - CRM (اضافه شده برای رفع خطا)
    public double? MinRecoveryPercentage { get; set; } = 90.0; // حداقل درصد بازیابی (مثلاً ۹۰٪)
    public double? MaxRecoveryPercentage { get; set; } = 110.0; // حداکثر درصد بازیابی (مثلاً ۱۱۰٪)
}