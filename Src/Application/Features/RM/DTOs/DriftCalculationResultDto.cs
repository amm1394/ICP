namespace Icp.Application.Features.RM.DTOs;

public class DriftCalculationResultDto
{
    public List<DriftFactorDto> ElementResults { get; set; } = new();
    public string Summary => $"{ElementResults.Count} elements drift-corrected.";
}

public record DriftFactorDto
{
    public string ElementSymbol { get; init; } = null!;
    public List<DateTime> RM_Times { get; init; } = new();
    public List<double> RM_Measured { get; init; } = new();
    public double R2 { get; init; }
    public List<double> CorrectionFactors { get; init; } = new(); // برای هر نمونه
}