using System.Text.Json;
using System.Text.RegularExpressions;
using Application.DTOs;
using Application.Services;
using Infrastructure.Persistence;
using MathNet.Numerics;
using MathNet.Numerics.LinearRegression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Wrapper;

namespace Infrastructure.Services;

/// <summary>
/// Implementation of drift correction algorithms
/// Based on Python RM_check. py logic
/// </summary>
public class DriftCorrectionService : IDriftCorrectionService
{
    private readonly IsatisDbContext _db;
    private readonly ILogger<DriftCorrectionService> _logger;

    // Default patterns for standard detection
    private const string DefaultBasePattern = @"^(BASE|STD|STANDARD)";
    private const string DefaultConePattern = @"^(CONE|CAL)";
    private static readonly Regex RmPattern = new(@"^(OREAS|SRM|CRM|STANDARD|STD)\s*\d*", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public DriftCorrectionService(IsatisDbContext db, ILogger<DriftCorrectionService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<Result<DriftCorrectionResult>> AnalyzeDriftAsync(DriftCorrectionRequest request)
    {
        try
        {
            var project = await _db.Projects
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProjectId == request.ProjectId);

            if (project == null)
                return Result<DriftCorrectionResult>.Fail("Project not found");

            // Get raw data
            var rawData = await GetParsedDataAsync(request.ProjectId);
            if (!rawData.Any())
                return Result<DriftCorrectionResult>.Fail("No data found for project");

            // Detect segments
            var segments = DetectSegments(rawData, request.BasePattern, request.ConePattern);

            // Get elements to analyze
            var elements = request.SelectedElements ?? GetAllElements(rawData);

            // Calculate drift for each element
            var elementDrifts = new Dictionary<string, ElementDriftInfo>();
            foreach (var element in elements)
            {
                var driftInfo = CalculateElementDrift(rawData, element, segments);
                if (driftInfo != null)
                    elementDrifts[element] = driftInfo;
            }

            // Build result (analysis only, no correction applied)
            var result = new DriftCorrectionResult(
                TotalSamples: rawData.Count,
                CorrectedSamples: 0,
                SegmentsFound: segments.Count,
                Segments: segments,
                ElementDrifts: elementDrifts,
                CorrectedData: new List<CorrectedSampleDto>()
            );

            return Result<DriftCorrectionResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to analyze drift for project {ProjectId}", request.ProjectId);
            return Result<DriftCorrectionResult>.Fail($"Failed to analyze drift: {ex.Message}");
        }
    }

    public async Task<Result<DriftCorrectionResult>> ApplyDriftCorrectionAsync(DriftCorrectionRequest request)
    {
        try
        {
            var project = await _db.Projects
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProjectId == request.ProjectId);

            if (project == null)
                return Result<DriftCorrectionResult>.Fail("Project not found");

            var rawData = await GetParsedDataAsync(request.ProjectId);
            if (!rawData.Any())
                return Result<DriftCorrectionResult>.Fail("No data found for project");

            // Detect segments
            var segments = DetectSegments(rawData, request.BasePattern, request.ConePattern);

            // Get elements
            var elements = request.SelectedElements ?? GetAllElements(rawData);

            // Calculate drift info
            var elementDrifts = new Dictionary<string, ElementDriftInfo>();
            foreach (var element in elements)
            {
                var driftInfo = CalculateElementDrift(rawData, element, segments);
                if (driftInfo != null)
                    elementDrifts[element] = driftInfo;
            }

            // Apply correction based on method
            var correctedData = request.Method switch
            {
                DriftMethod.Linear => ApplyLinearCorrection(rawData, segments, elements, elementDrifts),
                DriftMethod.Stepwise => ApplyStepwiseCorrection(rawData, segments, elements, elementDrifts),
                DriftMethod.Polynomial => ApplyPolynomialCorrection(rawData, elements),
                _ => rawData.Select((d, i) => new CorrectedSampleDto(
                    d.SolutionLabel,
                    i,
                    0,
                    d.Values,
                    d.Values,
                    new Dictionary<string, decimal>()
                )).ToList()
            };

            var result = new DriftCorrectionResult(
                TotalSamples: rawData.Count,
                CorrectedSamples: correctedData.Count(c => c.CorrectionFactors.Any()),
                SegmentsFound: segments.Count,
                Segments: segments,
                ElementDrifts: elementDrifts,
                CorrectedData: correctedData
            );

            return Result<DriftCorrectionResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to apply drift correction for project {ProjectId}", request.ProjectId);
            return Result<DriftCorrectionResult>.Fail($"Failed to apply drift correction: {ex.Message}");
        }
    }

    public async Task<Result<List<DriftSegment>>> DetectSegmentsAsync(
        Guid projectId,
        string? basePattern = null,
        string? conePattern = null)
    {
        try
        {
            var rawData = await GetParsedDataAsync(projectId);
            if (!rawData.Any())
                return Result<List<DriftSegment>>.Fail("No data found");

            var segments = DetectSegments(rawData, basePattern, conePattern);
            return Result<List<DriftSegment>>.Success(segments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to detect segments for project {ProjectId}", projectId);
            return Result<List<DriftSegment>>.Fail($"Failed to detect segments: {ex.Message}");
        }
    }

    public async Task<Result<Dictionary<string, List<decimal>>>> CalculateDriftRatiosAsync(
        Guid projectId,
        List<string>? elements = null)
    {
        try
        {
            var rawData = await GetParsedDataAsync(projectId);
            if (!rawData.Any())
                return Result<Dictionary<string, List<decimal>>>.Fail("No data found");

            var allElements = elements ?? GetAllElements(rawData);
            var ratios = new Dictionary<string, List<decimal>>();

            // Find standard samples (RM samples)
            var standardIndices = rawData
                .Select((d, i) => (Data: d, Index: i))
                .Where(x => IsStandardSample(x.Data.SolutionLabel))
                .Select(x => x.Index)
                .ToList();

            foreach (var element in allElements)
            {
                var elementRatios = new List<decimal>();

                for (int i = 1; i < standardIndices.Count; i++)
                {
                    var prevIdx = standardIndices[i - 1];
                    var currIdx = standardIndices[i];

                    var prevValue = GetElementValue(rawData[prevIdx], element);
                    var currValue = GetElementValue(rawData[currIdx], element);

                    if (prevValue.HasValue && currValue.HasValue && prevValue.Value != 0)
                    {
                        var ratio = currValue.Value / prevValue.Value;
                        elementRatios.Add(ratio);
                    }
                }

                ratios[element] = elementRatios;
            }

            return Result<Dictionary<string, List<decimal>>>.Success(ratios);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to calculate drift ratios for project {ProjectId}", projectId);
            return Result<Dictionary<string, List<decimal>>>.Fail($"Failed to calculate drift ratios: {ex.Message}");
        }
    }

    public async Task<Result<SlopeOptimizationResult>> OptimizeSlopeAsync(SlopeOptimizationRequest request)
    {
        try
        {
            var rawData = await GetParsedDataAsync(request.ProjectId);
            if (!rawData.Any())
                return Result<SlopeOptimizationResult>.Fail("No data found");

            // Get values for the element
            var values = rawData
                .Select((d, i) => (Index: i, Value: GetElementValue(d, request.Element)))
                .Where(x => x.Value.HasValue)
                .ToList();

            if (values.Count < 2)
                return Result<SlopeOptimizationResult>.Fail("Not enough data points for slope optimization");

            // Fit linear regression
            var xData = values.Select(v => (double)v.Index).ToArray();
            var yData = values.Select(v => (double)v.Value!.Value).ToArray();

            var (intercept, slope) = Fit.Line(xData, yData);

            // Calculate new slope based on action
            double newSlope = request.Action switch
            {
                SlopeAction.ZeroSlope => 0,
                SlopeAction.RotateUp => slope + Math.Abs(slope) * 0.1,
                SlopeAction.RotateDown => slope - Math.Abs(slope) * 0.1,
                SlopeAction.SetCustom => (double)(request.TargetSlope ?? 0),
                _ => slope
            };

            // Calculate new intercept to maintain center point
            var centerX = xData.Average();
            var centerY = yData.Average();
            var newIntercept = centerY - newSlope * centerX;

            // Apply correction
            var correctedData = new List<CorrectedSampleDto>();
            for (int i = 0; i < rawData.Count; i++)
            {
                var originalValue = GetElementValue(rawData[i], request.Element);
                var correctionFactor = 1.0m;

                if (originalValue.HasValue)
                {
                    var originalFitted = intercept + slope * i;
                    var newFitted = newIntercept + newSlope * i;

                    if (originalFitted != 0)
                        correctionFactor = (decimal)(newFitted / originalFitted);
                }

                var correctedValues = new Dictionary<string, decimal?>(rawData[i].Values);
                if (originalValue.HasValue)
                    correctedValues[request.Element] = originalValue.Value * correctionFactor;

                correctedData.Add(new CorrectedSampleDto(
                    rawData[i].SolutionLabel,
                    i,
                    0,
                    rawData[i].Values,
                    correctedValues,
                    new Dictionary<string, decimal> { { request.Element, correctionFactor } }
                ));
            }

            return Result<SlopeOptimizationResult>.Success(new SlopeOptimizationResult(
                request.Element,
                (decimal)slope,
                (decimal)newSlope,
                (decimal)intercept,
                (decimal)newIntercept,
                correctedData
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to optimize slope for project {ProjectId}", request.ProjectId);
            return Result<SlopeOptimizationResult>.Fail($"Failed to optimize slope: {ex.Message}");
        }
    }

    public async Task<Result<SlopeOptimizationResult>> ZeroSlopeAsync(Guid projectId, string element)
    {
        return await OptimizeSlopeAsync(new SlopeOptimizationRequest(projectId, element, SlopeAction.ZeroSlope));
    }

    #region Private Helper Methods

    private async Task<List<ParsedRow>> GetParsedDataAsync(Guid projectId)
    {
        var rawRows = await _db.RawDataRows
            .AsNoTracking()
            .Where(r => r.ProjectId == projectId)
            .OrderBy(r => r.DataId)
            .ToListAsync();

        var result = new List<ParsedRow>();
        foreach (var row in rawRows)
        {
            try
            {
                var data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(row.ColumnData);
                if (data == null) continue;

                var solutionLabel = data.TryGetValue("Solution Label", out var sl)
                    ? sl.GetString() ?? row.SampleId ?? $"Row_{row.DataId}"
                    : row.SampleId ?? $"Row_{row.DataId}";

                var values = new Dictionary<string, decimal?>();
                foreach (var kvp in data)
                {
                    if (kvp.Key == "Solution Label") continue;

                    if (kvp.Value.ValueKind == JsonValueKind.Number)
                    {
                        values[kvp.Key] = kvp.Value.GetDecimal();
                    }
                    else if (kvp.Value.ValueKind == JsonValueKind.String)
                    {
                        if (decimal.TryParse(kvp.Value.GetString(), out var val))
                            values[kvp.Key] = val;
                    }
                }

                result.Add(new ParsedRow(solutionLabel, values));
            }
            catch
            {
                // Skip malformed rows
            }
        }

        return result;
    }

    private List<DriftSegment> DetectSegments(List<ParsedRow> data, string? basePattern, string? conePattern)
    {
        var segments = new List<DriftSegment>();
        var baseRegex = new Regex(basePattern ?? DefaultBasePattern, RegexOptions.IgnoreCase);
        var coneRegex = new Regex(conePattern ?? DefaultConePattern, RegexOptions.IgnoreCase);

        var standardIndices = new List<(int Index, string Label, string Type)>();

        for (int i = 0; i < data.Count; i++)
        {
            var label = data[i].SolutionLabel;

            if (baseRegex.IsMatch(label))
                standardIndices.Add((i, label, "Base"));
            else if (coneRegex.IsMatch(label))
                standardIndices.Add((i, label, "Cone"));
            else if (RmPattern.IsMatch(label))
                standardIndices.Add((i, label, "RM"));
        }

        // Create segments between standards
        if (standardIndices.Count < 2)
        {
            // Single segment for entire data
            segments.Add(new DriftSegment(0, 0, data.Count - 1, null, null, data.Count));
        }
        else
        {
            for (int i = 0; i < standardIndices.Count - 1; i++)
            {
                var start = standardIndices[i];
                var end = standardIndices[i + 1];

                segments.Add(new DriftSegment(
                    i,
                    start.Index,
                    end.Index,
                    start.Label,
                    end.Label,
                    end.Index - start.Index
                ));
            }
        }

        return segments;
    }

    private List<string> GetAllElements(List<ParsedRow> data)
    {
        return data
            .SelectMany(d => d.Values.Keys)
            .Distinct()
            .OrderBy(e => e)
            .ToList();
    }

    private ElementDriftInfo? CalculateElementDrift(List<ParsedRow> data, string element, List<DriftSegment> segments)
    {
        var values = data
            .Select((d, i) => (Index: i, Value: GetElementValue(d, element)))
            .Where(x => x.Value.HasValue)
            .ToList();

        if (values.Count < 2)
            return null;

        var xData = values.Select(v => (double)v.Index).ToArray();
        var yData = values.Select(v => (double)v.Value!.Value).ToArray();

        var (intercept, slope) = Fit.Line(xData, yData);

        var firstValue = yData.First();
        var lastValue = yData.Last();
        var driftPercent = firstValue != 0 ? ((lastValue - firstValue) / firstValue) * 100 : 0;

        return new ElementDriftInfo(
            element,
            (decimal)(firstValue / (intercept + slope * xData.First())),
            (decimal)(lastValue / (intercept + slope * xData.Last())),
            (decimal)driftPercent,
            (decimal)slope,
            (decimal)intercept
        );
    }

    private List<CorrectedSampleDto> ApplyLinearCorrection(
        List<ParsedRow> data,
        List<DriftSegment> segments,
        List<string> elements,
        Dictionary<string, ElementDriftInfo> elementDrifts)
    {
        var result = new List<CorrectedSampleDto>();

        for (int i = 0; i < data.Count; i++)
        {
            var segment = segments.FirstOrDefault(s => i >= s.StartIndex && i <= s.EndIndex);
            var segmentIndex = segment?.SegmentIndex ?? 0;

            var correctedValues = new Dictionary<string, decimal?>();
            var correctionFactors = new Dictionary<string, decimal>();

            foreach (var element in elements)
            {
                var originalValue = GetElementValue(data[i], element);

                if (originalValue.HasValue && elementDrifts.TryGetValue(element, out var driftInfo))
                {
                    // Linear interpolation within segment
                    decimal factor = 1.0m;

                    if (segment != null && segment.EndIndex != segment.StartIndex)
                    {
                        var progress = (decimal)(i - segment.StartIndex) / (segment.EndIndex - segment.StartIndex);
                        var driftAtPoint = driftInfo.InitialRatio + (driftInfo.FinalRatio - driftInfo.InitialRatio) * progress;

                        if (driftAtPoint != 0)
                            factor = 1.0m / driftAtPoint;
                    }

                    correctedValues[element] = originalValue.Value * factor;
                    correctionFactors[element] = factor;
                }
                else
                {
                    correctedValues[element] = originalValue;
                }
            }

            result.Add(new CorrectedSampleDto(
                data[i].SolutionLabel,
                i,
                segmentIndex,
                data[i].Values,
                correctedValues,
                correctionFactors
            ));
        }

        return result;
    }

    private List<CorrectedSampleDto> ApplyStepwiseCorrection(
        List<ParsedRow> data,
        List<DriftSegment> segments,
        List<string> elements,
        Dictionary<string, ElementDriftInfo> elementDrifts)
    {
        var result = new List<CorrectedSampleDto>();
        var currentFactors = elements.ToDictionary(e => e, e => 1.0m);

        for (int i = 0; i < data.Count; i++)
        {
            var segment = segments.FirstOrDefault(s => i >= s.StartIndex && i <= s.EndIndex);
            var segmentIndex = segment?.SegmentIndex ?? 0;

            // Update factors at segment boundaries
            if (segment != null && i == segment.StartIndex && segmentIndex > 0)
            {
                foreach (var element in elements)
                {
                    if (elementDrifts.TryGetValue(element, out var driftInfo))
                    {
                        var segmentDrift = driftInfo.FinalRatio / driftInfo.InitialRatio;
                        var stepFactor = 1.0m / (decimal)Math.Pow((double)segmentDrift, 1.0 / segments.Count);
                        currentFactors[element] *= stepFactor;
                    }
                }
            }

            var correctedValues = new Dictionary<string, decimal?>();
            var correctionFactors = new Dictionary<string, decimal>();

            foreach (var element in elements)
            {
                var originalValue = GetElementValue(data[i], element);
                var factor = currentFactors[element];

                correctedValues[element] = originalValue.HasValue ? originalValue.Value * factor : null;
                correctionFactors[element] = factor;
            }

            result.Add(new CorrectedSampleDto(
                data[i].SolutionLabel,
                i,
                segmentIndex,
                data[i].Values,
                correctedValues,
                correctionFactors
            ));
        }

        return result;
    }

    private List<CorrectedSampleDto> ApplyPolynomialCorrection(List<ParsedRow> data, List<string> elements)
    {
        var result = new List<CorrectedSampleDto>();

        // Calculate polynomial fit for each element
        var polynomialFits = new Dictionary<string, (double[] Coefficients, double Mean)>();

        foreach (var element in elements)
        {
            var values = data
                .Select((d, i) => (Index: i, Value: GetElementValue(d, element)))
                .Where(x => x.Value.HasValue)
                .ToList();

            if (values.Count >= 3)
            {
                var xData = values.Select(v => (double)v.Index).ToArray();
                var yData = values.Select(v => (double)v.Value!.Value).ToArray();

                // Fit 2nd degree polynomial
                var coefficients = Fit.Polynomial(xData, yData, 2);
                var mean = yData.Average();

                polynomialFits[element] = (coefficients, mean);
            }
        }

        for (int i = 0; i < data.Count; i++)
        {
            var correctedValues = new Dictionary<string, decimal?>();
            var correctionFactors = new Dictionary<string, decimal>();

            foreach (var element in elements)
            {
                var originalValue = GetElementValue(data[i], element);

                if (originalValue.HasValue && polynomialFits.TryGetValue(element, out var fit))
                {
                    var fittedValue = fit.Coefficients[0] + fit.Coefficients[1] * i + fit.Coefficients[2] * i * i;
                    var factor = fittedValue != 0 ? (decimal)(fit.Mean / fittedValue) : 1.0m;

                    correctedValues[element] = originalValue.Value * factor;
                    correctionFactors[element] = factor;
                }
                else
                {
                    correctedValues[element] = originalValue;
                }
            }

            result.Add(new CorrectedSampleDto(
                data[i].SolutionLabel,
                i,
                0,
                data[i].Values,
                correctedValues,
                correctionFactors
            ));
        }

        return result;
    }

    private bool IsStandardSample(string label)
    {
        return RmPattern.IsMatch(label);
    }

    private decimal? GetElementValue(ParsedRow row, string element)
    {
        return row.Values.TryGetValue(element, out var value) ? value : null;
    }

    private record ParsedRow(string SolutionLabel, Dictionary<string, decimal?> Values);

    #endregion
}