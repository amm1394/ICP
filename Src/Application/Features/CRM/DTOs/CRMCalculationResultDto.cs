namespace Icp.Application.Features.CRM.DTOs;

public class CRMCalculationResultDto
{
    public List<CRMFactorDto> ElementResults { get; set; } = new();
    public string Summary => $"{ElementResults.Count} elements corrected.";
    public bool IsSuccessful => ElementResults.All(e => e.RecoveryPercent is >= 80 and <= 120);
}