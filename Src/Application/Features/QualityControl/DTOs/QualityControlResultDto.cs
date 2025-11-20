namespace Application.Features.QualityControl.DTOs;

public class QualityControlResultDto
{
    public int TotalChecked { get; set; }
    public int DeletedCount { get; set; }
    public List<DeletedSampleDto> DeletedSamples { get; set; } = new();
    public string Summary => $"Deleted {DeletedCount} samples from {TotalChecked}";
}

public record DeletedSampleDto(string SampleId, string Reason);