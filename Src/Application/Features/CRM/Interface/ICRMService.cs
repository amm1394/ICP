using Icp.Application.Features.CRM.DTOs;

namespace Icp.Application.Features.CRM.Interface;

public interface ICRMService
{
    Task<CRMCalculationResultDto> CalculateBlankAndScaleAsync(Guid projectId, CancellationToken ct = default);
    Task UndoCRMCorrectionAsync(Guid projectId, CancellationToken ct = default);
}