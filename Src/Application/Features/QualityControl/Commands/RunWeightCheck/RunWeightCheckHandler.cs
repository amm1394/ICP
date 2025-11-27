using Application.Features.QualityControl.Commands.RunWeightCheck;
using Domain.Enums;
using Domain.Interfaces.Services;
using MediatR;
using Shared.Wrapper;

public class RunWeightCheckHandler(IQualityControlService qcService)
    : IRequestHandler<RunWeightCheckCommand, Result<int>>
{
    public async Task<Result<int>> Handle(RunWeightCheckCommand request, CancellationToken cancellationToken)
    {
        // استفاده از سرویس مرکزی به جای نوشتن دوباره منطق
        var failedCount = await qcService.RunCheckAsync(
            request.ProjectId,
            CheckType.WeightCheck,
            cancellationToken);

        return await Result<int>.SuccessAsync(failedCount, $"Weight check completed. {failedCount} samples failed.");
    }
}