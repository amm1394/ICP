namespace Domain.Reports.DTOs;

public class PivotReportDto
{
    public Guid ProjectId { get; set; }

    // لیست نام عناصر (هدر ستون‌ها)
    public List<string> Columns { get; set; } = new();

    public List<PivotRowDto> Rows { get; set; } = new();
}