namespace Domain.Reports.DTOs;

public class ElementResultDto
{
    public double Value { get; set; } // غلظت میانگین
    public double RSD { get; set; }   // انحراف معیار نسبی (درصد)
    public string? Note { get; set; } // یادداشت اختیاری (مثلاً "< LOD")
}