namespace Icp.Application.Features.CRM.DTOs;

public record CRMFactorDto
{
    public string ElementSymbol { get; init; } = null!;
    public double BlankAverage { get; init; }
    public double MeasuredAverage { get; init; }
    public double ScaleFactor { get; init; }
    public double RecoveryPercent { get; init; }
}