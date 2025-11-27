namespace Domain.Reports.DTOs;

public class PivotRowDto
{
    public Guid SampleId { get; set; }
    public string SolutionLabel { get; set; } = string.Empty;
    public string SampleType { get; set; } = string.Empty; // نام استاندارد

    // ✅ فیلدهایی که خطا می‌دادند اضافه شدند
    public double Weight { get; set; }
    public double Volume { get; set; }
    public double DilutionFactor { get; set; }

    public int ReplicateCount { get; set; }

    // ✅ نوع دیکشنری اصلاح شد تا با ElementResultDto هماهنگ باشد
    public Dictionary<string, ElementResultDto> ElementValues { get; set; } = new();
}