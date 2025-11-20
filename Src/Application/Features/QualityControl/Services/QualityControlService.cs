using Application.Features.QualityControl.DTOs;
using Core.Icp.Domain.Entities;
using Core.Icp.Domain.Interfaces;
using Icp.Application.Features.QualityControl.Interface;

namespace Icp.Application.Features.QualityControl.Services;

public class QualityControlService : IQualityControlService
{
    private readonly IProjectRepository _projectRepository;

    public QualityControlService(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<QualityControlResultDto> RunAllChecksAsync(Guid projectId, CancellationToken ct = default)
    {
        var project = await _projectRepository.GetWithSamplesAsync(projectId, ct)
                      ?? throw new InvalidOperationException("Project not found.");

        var result = new QualityControlResultDto
        {
            TotalChecked = project.Samples.Count(s => !s.IsDeleted)
        };

        RunWeightCheck(project, result);
        RunVolumeCheck(project, result);
        RunDilutionFactorCheck(project, result);
        RunEmptyRowCheck(project, result);

        // اگر UnitOfWork داری این رو باز کن، وگرنه بعداً اضافه می‌کنیم
        // if (_projectRepository is IUnitOfWork uow)
        //     await uow.SaveChangesAsync(ct);

        return result;
    }

    private static void RunWeightCheck(Project project, QualityControlResultDto result)
    {
        var invalid = project.Samples
            .Where(s => !s.IsDeleted && (s.Weight is null or <= 0 or >= 10)) // >10 گرم غیرعادی
            .ToList();

        DeleteSamples(invalid, "Weight out of range (0 < Weight < 10 g)", result);
    }

    private static void RunVolumeCheck(Project project, QualityControlResultDto result)
    {
        var invalid = project.Samples
            .Where(s => !s.IsDeleted && (s.Volume is null or <= 0 or >= 100)) // >100 میلی‌لیتر غیرعادی
            .ToList();

        DeleteSamples(invalid, "Volume out of range (0 < Volume < 100 ml)", result);
    }

    private static void RunDilutionFactorCheck(Project project, QualityControlResultDto result)
    {
        var invalid = project.Samples
            .Where(s => !s.IsDeleted && (s.DilutionFactor.Value <= 0 || s.DilutionFactor.Value > 1000))
            .ToList();

        DeleteSamples(invalid, "Dilution Factor invalid (0 < DF ≤ 1000)", result);
    }

    private static void RunEmptyRowCheck(Project project, QualityControlResultDto result)
    {
        var empty = project.Samples
            .Where(s => !s.IsDeleted &&
                   (s.Measurements.Count == 0 || s.Measurements.All(m => m.NetIntensity == null || m.NetIntensity <= 0)))
            .ToList();

        DeleteSamples(empty, "Empty row (no intensity data)", result);
    }

    private static void DeleteSamples(List<Sample> samples, string reason, QualityControlResultDto result)
    {
        foreach (var sample in samples)
        {
            sample.MarkAsDeleted(reason);
            result.DeletedSamples.Add(new DeletedSampleDto(sample.SampleId, reason));
        }

        result.DeletedCount += samples.Count;
    }
}