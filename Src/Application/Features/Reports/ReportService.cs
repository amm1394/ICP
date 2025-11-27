using Domain.Entities;
using Domain.Interfaces;
using Domain.Reports.DTOs;

namespace Application.Features.Reports;

public class ReportService(IUnitOfWork unitOfWork) : IReportService
{
    public async Task<PivotReportDto> GetPivotReportAsync(Guid projectId, bool useConcentration = true, CancellationToken cancellationToken = default)
    {
        // 1. دریافت داده‌ها
        var samples = await unitOfWork.Repository<Sample>()
            .GetAsync(s => s.ProjectId == projectId, includeProperties: "Measurements");

        // 2. استخراج لیست عناصر
        var allElements = samples
            .SelectMany(s => s.Measurements)
            .Select(m => m.ElementName)
            .Distinct()
            .OrderBy(e => e)
            .ToList();

        // 3. گروه‌بندی بر اساس نام نمونه (میانگین‌گیری از تکرارها)
        var groupedSamples = samples.GroupBy(s => s.SolutionLabel).ToList();
        var rows = new List<PivotRowDto>();

        foreach (var group in groupedSamples)
        {
            var firstSample = group.First();

            var row = new PivotRowDto
            {
                SampleId = firstSample.Id,
                SolutionLabel = group.Key,
                SampleType = firstSample.Type.ToString(),
                Weight = group.Average(s => s.Weight),
                Volume = group.Average(s => s.Volume),
                DilutionFactor = firstSample.DilutionFactor,
                ReplicateCount = group.Count(),
                ElementValues = new Dictionary<string, ElementResultDto>()
            };

            foreach (var element in allElements)
            {
                var measurements = group
                    .SelectMany(s => s.Measurements)
                    .Where(m => m.ElementName == element)
                    .Select(m => useConcentration ? m.Concentration : m.Value)
                    .Where(v => v.HasValue)
                    .Select(v => v!.Value)
                    .ToList();

                if (measurements.Any())
                {
                    double mean = measurements.Average();
                    double rsd = 0;

                    if (measurements.Count > 1 && mean != 0)
                    {
                        double sumSquares = measurements.Sum(d => Math.Pow(d - mean, 2));
                        double stdDev = Math.Sqrt(sumSquares / (measurements.Count - 1));
                        rsd = (stdDev / mean) * 100;
                    }

                    row.ElementValues[element] = new ElementResultDto
                    {
                        Value = mean,
                        RSD = rsd,
                        Note = null
                    };
                }
            }
            rows.Add(row);
        }

        return new PivotReportDto
        {
            ProjectId = projectId,
            Columns = allElements,
            Rows = rows
        };
    }
}