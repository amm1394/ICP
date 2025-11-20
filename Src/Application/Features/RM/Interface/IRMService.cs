using Icp.Application.Features.RM.DTOs;

namespace Icp.Application.Features.RM.Interface;

public interface IRMService
{
    Task<DriftCalculationResultDto> CalculateDriftCorrectionAsync(Guid projectId, CancellationToken ct = default);
    Task UndoDriftCorrectionAsync(Guid projectId, CancellationToken ct = default);
}