using Core.Icp.Domain.Entities;
using Core.Icp.Domain.Interfaces;
using Icp.Application.Features.CRM.DTOs;
using Icp.Application.Features.CRM.Interface;

namespace Icp.Application.Features.CRM.Services;

public class CRMService : ICRMService
{
    private readonly IProjectRepository _projectRepository;
    private readonly Stack<CRMCorrectionSnapshot> _undoStack = new();

    public CRMService(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<CRMCalculationResultDto> CalculateBlankAndScaleAsync(Guid projectId, CancellationToken ct = default)
    {
        // فعلاً از همین متد استفاده می‌کنیم – بعداً Include Measurements اضافه می‌کنیم
        var project = await _projectRepository.GetWithSamplesAsync(projectId, ct)
                      ?? throw new InvalidOperationException("Project not found");

        var result = new CRMCalculationResultDto();

        var blankSamples = project.Samples
            .Where(s => !s.IsDeleted && s.SampleName.Contains("BLK", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var crmSamples = project.Samples
            .Where(s => !s.IsDeleted && s.SampleName.Contains("CRM", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (!crmSamples.Any())
            throw new InvalidOperationException("No CRM samples found.");

        var selectedElements = project.SelectedElements.Where(e => e.IsSelected).ToList();

        // دیکشنری با string کلید (Symbol)
        var blankAverages = CalculateBlankAverages(blankSamples, selectedElements);
        var scaleFactors = CalculateScaleFactors(crmSamples, blankAverages, selectedElements, result);

        ApplyBlankAndScaleCorrection(project, blankAverages, scaleFactors);

        _undoStack.Push(new CRMCorrectionSnapshot(project));

        return result;
    }

    private static Dictionary<string, double> CalculateBlankAverages(List<Sample> blankSamples, List<Element> elements)
    {
        var averages = new Dictionary<string, double>();

        foreach (var element in elements)
        {
            var values = blankSamples
                .SelectMany(s => s.Measurements
                    .Where(m => m.ElementSymbol == element.Symbol)
                    .Select(m => m.Concentration ?? 0))
                .ToList();

            averages[element.Symbol] = values.Any() ? values.Average() : 0;
        }

        return averages;
    }

    private static Dictionary<string, double> CalculateScaleFactors(List<Sample> crmSamples, Dictionary<string, double> blankAverages, List<Element> elements, CRMCalculationResultDto result)
    {
        var factors = new Dictionary<string, double>();

        foreach (var element in elements)
        {
            double certifiedValue = 100.0; // TODO: از UI یا دیتابیس بگیر

            var measuredValues = crmSamples
                .SelectMany(s => s.Measurements
                    .Where(m => m.ElementSymbol == element.Symbol)
                    .Select(m => (m.Concentration ?? 0) - blankAverages.GetValueOrDefault(element.Symbol)))
                .Where(v => v > 0)
                .ToList();

            double measuredAvg = measuredValues.Any() ? measuredValues.Average() : 0;
            double scaleFactor = measuredAvg != 0 ? certifiedValue / measuredAvg : 1.0;

            factors[element.Symbol] = scaleFactor;

            double recovery = measuredAvg != 0 ? (measuredAvg / certifiedValue) * 100 : 0;

            result.ElementResults.Add(new CRMFactorDto
            {
                ElementSymbol = element.Symbol,
                BlankAverage = blankAverages.GetValueOrDefault(element.Symbol),
                MeasuredAverage = measuredAvg,
                ScaleFactor = scaleFactor,
                RecoveryPercent = recovery
            });
        }

        return factors;
    }

    private static void ApplyBlankAndScaleCorrection(Project project, Dictionary<string, double> blankAverages, Dictionary<string, double> scaleFactors)
    {
        foreach (var sample in project.Samples.Where(s => !s.IsDeleted && !s.SampleName.Contains("CRM", StringComparison.OrdinalIgnoreCase) && !s.SampleName.Contains("BLK", StringComparison.OrdinalIgnoreCase)))
        {
            foreach (var measurement in sample.Measurements)
            {
                double blank = blankAverages.GetValueOrDefault(measurement.ElementSymbol);
                double scale = scaleFactors.GetValueOrDefault(measurement.ElementSymbol, 1.0);

                double corrected = (measurement.Concentration ?? 0 - blank) * scale;
                measurement.FinalConcentration = corrected > 0 ? corrected : 0;
            }
        }
    }

    public Task UndoCRMCorrectionAsync(Guid projectId, CancellationToken ct = default)
    {
        if (_undoStack.TryPop(out var snapshot))
        {
            snapshot.Restore();
        }

        return Task.CompletedTask;
    }
}

// ساده‌تر شده Undo
internal class CRMCorrectionSnapshot
{
    private readonly Project _project;
    private readonly Dictionary<Guid, double?> _backup = new();

    public CRMCorrectionSnapshot(Project project)
    {
        _project = project;
        foreach (var sample in project.Samples)
        {
            foreach (var m in sample.Measurements)
            {
                _backup[m.Id] = m.FinalConcentration;
            }
        }
    }

    public void Restore()
    {
        foreach (var sample in _project.Samples)
        {
            foreach (var m in sample.Measurements)
            {
                if (_backup.TryGetValue(m.Id, out var value))
                    m.FinalConcentration = value;
            }
        }
    }
}