using System.Text.Json;
using Application.DTOs;
using Application.Services;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Wrapper;

namespace Infrastructure.Services;

/// <summary>
/// Implementation of Blank & Scale optimization using Differential Evolution
/// Based on Python scipy.optimize.differential_evolution
/// </summary>
public class OptimizationService : IOptimizationService
{
    private readonly IsatisDbContext _db;
    private readonly ILogger<OptimizationService> _logger;
    private readonly Random _random = new();

    public OptimizationService(IsatisDbContext db, ILogger<OptimizationService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<Result<BlankScaleOptimizationResult>> OptimizeBlankScaleAsync(BlankScaleOptimizationRequest request)
    {
        try
        {
            // Get project data and CRM data
            var projectData = await GetProjectRmDataAsync(request.ProjectId);
            if (!projectData.Any())
                return Result<BlankScaleOptimizationResult>.Fail("No RM samples found in project");

            var crmData = await GetCrmDataAsync();
            if (!crmData.Any())
                return Result<BlankScaleOptimizationResult>.Fail("No CRM data found");

            // Match project RM samples with CRM
            var matchedData = MatchWithCrm(projectData, crmData);
            if (!matchedData.Any())
                return Result<BlankScaleOptimizationResult>.Fail("No matching CRM data found for RM samples");

            // Get elements to optimize
            var elements = request.Elements ?? GetCommonElements(matchedData);

            // Calculate initial statistics
            var initialStats = CalculateStatistics(matchedData, elements, 0, 1, request.MinDiffPercent, request.MaxDiffPercent);

            // Optimize each element
            var elementOptimizations = new Dictionary<string, ElementOptimization>();
            var bestBlanks = new Dictionary<string, decimal>();
            var bestScales = new Dictionary<string, decimal>();

            foreach (var element in elements)
            {
                var (optimalBlank, optimalScale, passedAfter) = OptimizeElement(
                    matchedData,
                    element,
                    request.MinDiffPercent,
                    request.MaxDiffPercent,
                    request.MaxIterations,
                    request.PopulationSize
                );

                var passedBefore = initialStats.ElementStats.TryGetValue(element, out var stats) ? stats.Passed : 0;

                elementOptimizations[element] = new ElementOptimization(
                    element,
                    optimalBlank,
                    optimalScale,
                    passedBefore,
                    passedAfter,
                    stats?.MeanDiff ?? 0,
                    CalculateMeanDiff(matchedData, element, optimalBlank, optimalScale)
                );

                bestBlanks[element] = optimalBlank;
                bestScales[element] = optimalScale;
            }

            // Build optimized data
            var optimizedData = BuildOptimizedData(
                matchedData,
                elements,
                bestBlanks,
                bestScales,
                request.MinDiffPercent,
                request.MaxDiffPercent
            );

            var totalPassedBefore = elementOptimizations.Values.Sum(e => e.PassedBefore);
            var totalPassedAfter = elementOptimizations.Values.Sum(e => e.PassedAfter);
            var improvement = totalPassedBefore > 0
                ? ((decimal)(totalPassedAfter - totalPassedBefore) / totalPassedBefore) * 100
                : 0;

            var result = new BlankScaleOptimizationResult(
                matchedData.Count,
                totalPassedBefore,
                totalPassedAfter,
                improvement,
                elementOptimizations,
                optimizedData
            );

            return Result<BlankScaleOptimizationResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to optimize Blank & Scale for project {ProjectId}", request.ProjectId);
            return Result<BlankScaleOptimizationResult>.Fail("Failed to optimize: " + ex.Message);
        }
    }

    public async Task<Result<ManualBlankScaleResult>> ApplyManualBlankScaleAsync(ManualBlankScaleRequest request)
    {
        return await PreviewBlankScaleAsync(request);
    }

    public async Task<Result<ManualBlankScaleResult>> PreviewBlankScaleAsync(ManualBlankScaleRequest request)
    {
        try
        {
            var projectData = await GetProjectRmDataAsync(request.ProjectId);
            var crmData = await GetCrmDataAsync();
            var matchedData = MatchWithCrm(projectData, crmData);

            if (!matchedData.Any())
                return Result<ManualBlankScaleResult>.Fail("No matching CRM data found");

            var elements = new List<string> { request.Element };
            var blanks = new Dictionary<string, decimal> { { request.Element, request.Blank } };
            var scales = new Dictionary<string, decimal> { { request.Element, request.Scale } };

            var beforeStats = CalculateStatistics(matchedData, elements, 0, 1, -10, 10);
            var afterStats = CalculateStatistics(matchedData, elements, request.Blank, request.Scale, -10, 10);

            var optimizedData = BuildOptimizedData(matchedData, elements, blanks, scales, -10, 10);

            var passedBefore = beforeStats.ElementStats.TryGetValue(request.Element, out var bs) ? bs.Passed : 0;
            var passedAfter = afterStats.ElementStats.TryGetValue(request.Element, out var afs) ? afs.Passed : 0;

            return Result<ManualBlankScaleResult>.Success(new ManualBlankScaleResult(
                request.Element,
                request.Blank,
                request.Scale,
                passedBefore,
                passedAfter,
                optimizedData
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to preview Blank & Scale");
            return Result<ManualBlankScaleResult>.Fail("Failed to preview: " + ex.Message);
        }
    }

    public async Task<Result<BlankScaleOptimizationResult>> GetCurrentStatisticsAsync(
        Guid projectId,
        decimal minDiff = -10m,
        decimal maxDiff = 10m)
    {
        try
        {
            var projectData = await GetProjectRmDataAsync(projectId);
            var crmData = await GetCrmDataAsync();
            var matchedData = MatchWithCrm(projectData, crmData);

            if (!matchedData.Any())
                return Result<BlankScaleOptimizationResult>.Fail("No matching CRM data found");

            var elements = GetCommonElements(matchedData);
            var stats = CalculateStatistics(matchedData, elements, 0, 1, minDiff, maxDiff);

            var elementOptimizations = elements.ToDictionary(
                e => e,
                e => new ElementOptimization(
                    e,
                    0, 1,
                    stats.ElementStats.TryGetValue(e, out var s) ? s.Passed : 0,
                    stats.ElementStats.TryGetValue(e, out var s2) ? s2.Passed : 0,
                    s?.MeanDiff ?? 0,
                    s2?.MeanDiff ?? 0
                )
            );

            var blanks = elements.ToDictionary(e => e, e => 0m);
            var scales = elements.ToDictionary(e => e, e => 1m);
            var optimizedData = BuildOptimizedData(matchedData, elements, blanks, scales, minDiff, maxDiff);

            return Result<BlankScaleOptimizationResult>.Success(new BlankScaleOptimizationResult(
                matchedData.Count,
                stats.TotalPassed,
                stats.TotalPassed,
                0,
                elementOptimizations,
                optimizedData
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get current statistics");
            return Result<BlankScaleOptimizationResult>.Fail("Failed to get statistics: " + ex.Message);
        }
    }

    #region Differential Evolution Algorithm

    private (decimal Blank, decimal Scale, int Passed) OptimizeElement(
        List<MatchedSample> data,
        string element,
        decimal minDiff,
        decimal maxDiff,
        int maxIterations,
        int populationSize)
    {
        // Differential Evolution parameters
        const double F = 0.8;  // Mutation factor
        const double CR = 0.9; // Crossover probability

        // Bounds: Blank [-100, 100], Scale [0.5, 2. 0]
        var blankBounds = (-100.0, 100.0);
        var scaleBounds = (0.5, 2.0);

        // Initialize population
        var population = new List<(double Blank, double Scale)>();
        for (int i = 0; i < populationSize; i++)
        {
            population.Add((
                _random.NextDouble() * (blankBounds.Item2 - blankBounds.Item1) + blankBounds.Item1,
                _random.NextDouble() * (scaleBounds.Item2 - scaleBounds.Item1) + scaleBounds.Item1
            ));
        }

        // Evaluate fitness
        var fitness = population.Select(p => EvaluateFitness(data, element, (decimal)p.Blank, (decimal)p.Scale, minDiff, maxDiff)).ToList();

        // Evolution loop
        for (int iter = 0; iter < maxIterations; iter++)
        {
            for (int i = 0; i < populationSize; i++)
            {
                // Select 3 random individuals (different from i)
                var candidates = Enumerable.Range(0, populationSize).Where(j => j != i).OrderBy(_ => _random.Next()).Take(3).ToList();
                var a = population[candidates[0]];
                var b = population[candidates[1]];
                var c = population[candidates[2]];

                // Mutation
                var mutantBlank = a.Blank + F * (b.Blank - c.Blank);
                var mutantScale = a.Scale + F * (b.Scale - c.Scale);

                // Clamp to bounds
                mutantBlank = Math.Clamp(mutantBlank, blankBounds.Item1, blankBounds.Item2);
                mutantScale = Math.Clamp(mutantScale, scaleBounds.Item1, scaleBounds.Item2);

                // Crossover
                var trialBlank = _random.NextDouble() < CR ? mutantBlank : population[i].Blank;
                var trialScale = _random.NextDouble() < CR ? mutantScale : population[i].Scale;

                // Selection
                var trialFitness = EvaluateFitness(data, element, (decimal)trialBlank, (decimal)trialScale, minDiff, maxDiff);
                if (trialFitness > fitness[i])
                {
                    population[i] = (trialBlank, trialScale);
                    fitness[i] = trialFitness;
                }
            }
        }

        // Find best
        var bestIdx = fitness.IndexOf(fitness.Max());
        var best = population[bestIdx];

        return ((decimal)best.Blank, (decimal)best.Scale, (int)fitness[bestIdx]);
    }

    private double EvaluateFitness(List<MatchedSample> data, string element, decimal blank, decimal scale, decimal minDiff, decimal maxDiff)
    {
        int passed = 0;

        foreach (var sample in data)
        {
            if (!sample.SampleValues.TryGetValue(element, out var sampleValue) || !sampleValue.HasValue)
                continue;
            if (!sample.CrmValues.TryGetValue(element, out var crmValue) || !crmValue.HasValue || crmValue.Value == 0)
                continue;

            var correctedValue = (sampleValue.Value + blank) * scale;
            var diffPercent = ((correctedValue - crmValue.Value) / crmValue.Value) * 100;

            if (diffPercent >= minDiff && diffPercent <= maxDiff)
                passed++;
        }

        return passed;
    }

    #endregion

    #region Helper Methods

    private async Task<List<RmSampleData>> GetProjectRmDataAsync(Guid projectId)
    {
        var rawRows = await _db.RawDataRows
            .AsNoTracking()
            .Where(r => r.ProjectId == projectId)
            .ToListAsync();

        var result = new List<RmSampleData>();
        var rmPattern = new System.Text.RegularExpressions.Regex(@"^(OREAS|SRM|CRM)\s*\d*", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        foreach (var row in rawRows)
        {
            try
            {
                var data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(row.ColumnData);
                if (data == null) continue;

                var solutionLabel = data.TryGetValue("Solution Label", out var sl)
                    ? sl.GetString() ?? ""
                    : row.SampleId ?? "";

                if (!rmPattern.IsMatch(solutionLabel))
                    continue;

                var values = new Dictionary<string, decimal?>();
                foreach (var kvp in data)
                {
                    if (kvp.Key == "Solution Label") continue;
                    if (kvp.Value.ValueKind == JsonValueKind.Number)
                        values[kvp.Key] = kvp.Value.GetDecimal();
                    else if (kvp.Value.ValueKind == JsonValueKind.String && decimal.TryParse(kvp.Value.GetString(), out var val))
                        values[kvp.Key] = val;
                }

                result.Add(new RmSampleData(solutionLabel, values));
            }
            catch { }
        }

        return result;
    }

    private async Task<Dictionary<string, Dictionary<string, decimal>>> GetCrmDataAsync()
    {
        var crmRecords = await _db.CrmData.AsNoTracking().ToListAsync();
        var result = new Dictionary<string, Dictionary<string, decimal>>(StringComparer.OrdinalIgnoreCase);

        foreach (var crm in crmRecords)
        {
            try
            {
                var values = JsonSerializer.Deserialize<Dictionary<string, decimal>>(crm.ElementValues);
                if (values != null)
                    result[crm.CrmId] = values;
            }
            catch { }
        }

        return result;
    }

    private List<MatchedSample> MatchWithCrm(List<RmSampleData> projectData, Dictionary<string, Dictionary<string, decimal>> crmData)
    {
        var result = new List<MatchedSample>();

        foreach (var sample in projectData)
        {
            var matchedCrm = crmData.Keys.FirstOrDefault(k =>
                sample.SolutionLabel.Contains(k, StringComparison.OrdinalIgnoreCase) ||
                k.Contains(sample.SolutionLabel, StringComparison.OrdinalIgnoreCase));

            if (matchedCrm != null)
            {
                var crmValues = crmData[matchedCrm].ToDictionary(kvp => kvp.Key, kvp => (decimal?)kvp.Value);
                result.Add(new MatchedSample(sample.SolutionLabel, matchedCrm, sample.Values, crmValues));
            }
        }

        return result;
    }

    private List<string> GetCommonElements(List<MatchedSample> data)
    {
        var sampleElements = data.SelectMany(d => d.SampleValues.Keys).Distinct();
        var crmElements = data.SelectMany(d => d.CrmValues.Keys).Distinct();
        return sampleElements.Intersect(crmElements, StringComparer.OrdinalIgnoreCase).ToList();
    }

    private (int TotalPassed, Dictionary<string, ElementStats> ElementStats) CalculateStatistics(
        List<MatchedSample> data,
        List<string> elements,
        decimal blank,
        decimal scale,
        decimal minDiff,
        decimal maxDiff)
    {
        var elementStats = new Dictionary<string, ElementStats>();
        int totalPassed = 0;

        foreach (var element in elements)
        {
            int passed = 0;
            var diffs = new List<decimal>();

            foreach (var sample in data)
            {
                if (!sample.SampleValues.TryGetValue(element, out var sv) || !sv.HasValue) continue;
                if (!sample.CrmValues.TryGetValue(element, out var cv) || !cv.HasValue || cv.Value == 0) continue;

                var corrected = (sv.Value + blank) * scale;
                var diff = ((corrected - cv.Value) / cv.Value) * 100;
                diffs.Add(diff);

                if (diff >= minDiff && diff <= maxDiff)
                    passed++;
            }

            elementStats[element] = new ElementStats(passed, diffs.Any() ? diffs.Average() : 0);
            totalPassed += passed;
        }

        return (totalPassed, elementStats);
    }

    private decimal CalculateMeanDiff(List<MatchedSample> data, string element, decimal blank, decimal scale)
    {
        var diffs = new List<decimal>();

        foreach (var sample in data)
        {
            if (!sample.SampleValues.TryGetValue(element, out var sv) || !sv.HasValue) continue;
            if (!sample.CrmValues.TryGetValue(element, out var cv) || !cv.HasValue || cv.Value == 0) continue;

            var corrected = (sv.Value + blank) * scale;
            var diff = ((corrected - cv.Value) / cv.Value) * 100;
            diffs.Add(diff);
        }

        return diffs.Any() ? diffs.Average() : 0;
    }

    private List<OptimizedSampleDto> BuildOptimizedData(
        List<MatchedSample> data,
        List<string> elements,
        Dictionary<string, decimal> blanks,
        Dictionary<string, decimal> scales,
        decimal minDiff,
        decimal maxDiff)
    {
        var result = new List<OptimizedSampleDto>();

        foreach (var sample in data)
        {
            var optimizedValues = new Dictionary<string, decimal?>();
            var diffBefore = new Dictionary<string, decimal>();
            var diffAfter = new Dictionary<string, decimal>();
            var passBefore = new Dictionary<string, bool>();
            var passAfter = new Dictionary<string, bool>();

            foreach (var element in elements)
            {
                if (!sample.SampleValues.TryGetValue(element, out var sv)) continue;
                if (!sample.CrmValues.TryGetValue(element, out var cv) || !cv.HasValue || cv.Value == 0) continue;

                var blank = blanks.TryGetValue(element, out var b) ? b : 0;
                var scale = scales.TryGetValue(element, out var s) ? s : 1;

                var original = sv ?? 0;
                var optimized = (original + blank) * scale;
                optimizedValues[element] = optimized;

                var diffB = ((original - cv.Value) / cv.Value) * 100;
                var diffA = ((optimized - cv.Value) / cv.Value) * 100;

                diffBefore[element] = diffB;
                diffAfter[element] = diffA;
                passBefore[element] = diffB >= minDiff && diffB <= maxDiff;
                passAfter[element] = diffA >= minDiff && diffA <= maxDiff;
            }

            result.Add(new OptimizedSampleDto(
                sample.SolutionLabel,
                sample.CrmId,
                sample.SampleValues,
                sample.CrmValues,
                optimizedValues,
                diffBefore,
                diffAfter,
                passBefore,
                passAfter
            ));
        }

        return result;
    }

    private record RmSampleData(string SolutionLabel, Dictionary<string, decimal?> Values);
    private record MatchedSample(string SolutionLabel, string CrmId, Dictionary<string, decimal?> SampleValues, Dictionary<string, decimal?> CrmValues);
    private record ElementStats(int Passed, decimal MeanDiff);

    #endregion
}