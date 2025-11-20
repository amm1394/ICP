using Core.Icp.Domain.Entities;
using Core.Icp.Domain.Interfaces;
using Icp.Application.Features.RM.DTOs;
using Icp.Application.Features.RM.Interface;

// فقط usingهای MathNet – System.Numerics کاملاً حذف شد تا تداخل رفع بشه
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System.Linq; // برای Average و Sum

namespace Icp.Application.Features.RM.Services;

public class RMService : IRMService
{
    private readonly IProjectRepository _projectRepository;
    private readonly Stack<DriftCorrectionSnapshot> _undoStack = new();

    public RMService(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<DriftCalculationResultDto> CalculateDriftCorrectionAsync(Guid projectId, CancellationToken ct = default)
    {
        var project = await _projectRepository.GetWithSamplesAsync(projectId, ct)
                      ?? throw new InvalidOperationException("Project not found");

        var result = new DriftCalculationResultDto();

        var rmSamples = project.Samples
            .Where(s => !s.IsDeleted && s.SampleName.Contains("RM", StringComparison.OrdinalIgnoreCase))
            .OrderBy(s => s.RunDate)
            .ToList();

        if (rmSamples.Count < 2)
            throw new InvalidOperationException("At least 2 RM samples required for drift correction.");

        // اگر RunDate پر نشده باشد، از ترتیب نمونه‌ها رو بر اساس ترتیب در لیست فرض می‌کنیم
        if (rmSamples.All(s => s.RunDate == null))
        {
            // زمان مصنوعی بر اساس ترتیب
            for (int i = 0; i < rmSamples.Count; i++)
            {
                rmSamples[i].RunDate = rmSamples[0].RunDate ?? DateTime.MinValue.AddMinutes(i * 30); // هر ۳۰ دقیقه
            }
        }

        var selectedElements = project.SelectedElements.Where(e => e.IsSelected).ToList();

        foreach (var element in selectedElements)
        {
            var rmTimes = new List<double>();
            var rmMeasured = new List<double>();

            foreach (var rm in rmSamples)
            {
                var m = rm.Measurements.FirstOrDefault(m => m.ElementSymbol == element.Symbol);
                if (m?.FinalConcentration != null && rm.RunDate != null)
                {
                    double minutesFromStart = (rm.RunDate.Value - rmSamples[0].RunDate.Value).TotalMinutes;
                    rmTimes.Add(minutesFromStart);
                    rmMeasured.Add(m.FinalConcentration.Value);
                }
            }

            if (rmMeasured.Count < 2) continue;

            // Linear Regression با MathNet.Numerics (بدون تداخل)
            var x = DenseVector.Build.DenseOfArray(rmTimes.ToArray());
            var y = DenseVector.Build.DenseOfArray(rmMeasured.ToArray());

            var X = DenseMatrix.Build.DenseOfColumnVectors(x, DenseVector.Build.Dense(rmTimes.Count, 1.0)); // [x 1]
            var beta = X.QR().Solve(y); // slope, intercept

            double slope = beta[0];
            double intercept = beta[1];

            // R² محاسبه (با چک تقسیم بر صفر)
            double yMean = y.Average();
            double ssTot = y.Sum(yi => Math.Pow(yi - yMean, 2));
            double ssRes = 0;
            for (int i = 0; i < y.Count; i++)
            {
                double predicted = slope * rmTimes[i] + intercept;
                ssRes += Math.Pow(rmMeasured[i] - predicted, 2);
            }
            double r2 = ssTot > 0 ? 1 - (ssRes / ssTot) : 1.0;

            // اعمال correction به همه نمونه‌ها (غیر RM)
            var allSamples = project.Samples
                .Where(s => !s.IsDeleted && !s.SampleName.Contains("RM", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var correctionFactors = new List<double>();

            double referenceValue = rmMeasured.Average(); // بهتر از اولین RM استفاده کنیم میانگین

            foreach (var sample in allSamples)
            {
                if (sample.RunDate == null) continue;

                double minutes = (sample.RunDate.Value - rmSamples[0].RunDate.Value).TotalMinutes;
                double expected = slope * minutes + intercept;

                double factor = expected != 0 ? referenceValue / expected : 1.0;

                correctionFactors.Add(factor);

                var meas = sample.Measurements.FirstOrDefault(m => m.ElementSymbol == element.Symbol);
                if (meas?.FinalConcentration != null)
                {
                    meas.FinalConcentration *= factor;
                }
            }

            result.ElementResults.Add(new DriftFactorDto
            {
                ElementSymbol = element.Symbol,
                RM_Times = rmTimes.Select(t => rmSamples[0].RunDate.Value.AddMinutes(t)).ToList(),
                RM_Measured = rmMeasured,
                R2 = Math.Round(r2, 4),
                CorrectionFactors = correctionFactors
            });
        }

        _undoStack.Push(new DriftCorrectionSnapshot(project));

        return result;
    }

    public Task UndoDriftCorrectionAsync(Guid projectId, CancellationToken ct = default)
    {
        if (_undoStack.TryPop(out var snapshot))
        {
            snapshot.Restore();
        }

        return Task.CompletedTask;
    }
}

internal class DriftCorrectionSnapshot
{
    private readonly Project _project;
    private readonly Dictionary<Guid, double?> _backup = new();

    public DriftCorrectionSnapshot(Project project)
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
                {
                    m.FinalConcentration = value;
                }
            }
        }
    }
}