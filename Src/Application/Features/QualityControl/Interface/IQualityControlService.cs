using Application.Features.QualityControl.DTOs;

namespace Icp.Application.Features.QualityControl.Interface;

public interface IQualityControlService
{
    Task<QualityControlResultDto> RunAllChecksAsync(Guid projectId, CancellationToken ct = default);
}