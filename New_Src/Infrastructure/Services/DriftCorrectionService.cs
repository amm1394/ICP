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
/// Based on Python RM_check. py logic - Piecewise/Local approach for 100% compatibility
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

    #region Public Methods

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

            // Detect segments (Python-compatible: piecewise between standards)
            var segments = DetectSegments(rawData, request.BasePattern, request.ConePattern);

            // Get elements to analyze
            var elements = request.SelectedElements ?? GetAllElements(rawData);

            // Calculate segment-specific ratios (Python-compatible: local ratios)
            var segmentRatios = CalculateSegmentRatios(rawData, segments, elements);

            // Calculate drift info for each element
            var elementDrifts = new Dictionary<string, ElementDriftInfo>();
            foreach (var element in elements)
            {
                var driftInfo = CalculateElementDriftPiecewise(rawData, element, segments, segmentRatios);
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

            // Calculate segment-specific ratios (Python-compatible)
            var segmentRatios = CalculateSegmentRatios(rawData, segments, elements);

            // Calculate drift info
            var elementDrifts = new Dictionary<string, ElementDriftInfo>();
            foreach (var element in elements)
            {
                var driftInfo = CalculateElementDriftPiecewise(rawData, element, segments, segmentRatios);
                if (driftInfo != null)
                    elementDrifts[element] = driftInfo;
            }

            // Apply correction based on method (all methods now use Piecewise/Local approach)
            var correctedData = request.Method switch
            {
                DriftMethod.Linear => ApplyLinearCorrectionPiecewise(rawData, segments, elements, segmentRatios),
                DriftMethod.Stepwise => ApplyStepwiseCorrectionArithmetic(rawData, segments, elements, segmentRatios),
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

            _logger.LogInformation(
                "Drift correction applied: Method={Method}, Segments={Segments}, CorrectedSamples={Corrected}",
                request.Method, segments.Count, result.CorrectedSamples);

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

                // Python-compatible: Calculate ratio between consecutive standards
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

    #endregion

    #region Private Helper Methods - Data Access

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

    #endregion

    #region Private Helper Methods - Segment Detection

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

    private bool IsStandardSample(string label)
    {
        return RmPattern.IsMatch(label);
    }

    private decimal? GetElementValue(ParsedRow row, string element)
    {
        return row.Values.TryGetValue(element, out var value) ? value : null;
    }

    #endregion

    #region Private Helper Methods - Piecewise Calculations (Python-Compatible)

    /// <summary>
    /// Calculate drift ratios for each segment and element
    /// Python-compatible: calculates ratio between start and end of each segment (LOCAL approach)
    /// Python equivalent: ratio = end_standard_value / start_standard_value
    /// </summary>
    private Dictionary<int, Dictionary<string, decimal>> CalculateSegmentRatios(
        List<ParsedRow> data,
        List<DriftSegment> segments,
        List<string> elements)
    {
        var result = new Dictionary<int, Dictionary<string, decimal>>();

        foreach (var segment in segments)
        {
            var ratios = new Dictionary<string, decimal>();

            foreach (var element in elements)
            {
                var startValue = GetElementValue(data[segment.StartIndex], element);
                var endValue = GetElementValue(data[segment.EndIndex], element);

                if (startValue.HasValue && endValue.HasValue && startValue.Value != 0)
                {
                    // Python: ratio = end_value / start_value (LOCAL ratio for this segment only)
                    ratios[element] = endValue.Value / startValue.Value;
                }
                else
                {
                    ratios[element] = 1.0m; // No drift
                }
            }

            result[segment.SegmentIndex] = ratios;
        }

        return result;
    }

    /// <summary>
    /// Calculate element drift using piecewise approach (Python-compatible)
    /// Instead of global regression, calculates drift between consecutive standards
    /// </summary>
    private ElementDriftInfo? CalculateElementDriftPiecewise(
        List<ParsedRow> data,
        string element,
        List<DriftSegment> segments,
        Dictionary<int, Dictionary<string, decimal>> segmentRatios)
    {
        if (segments.Count == 0)
            return null;

        var firstSegment = segments.First();
        var lastSegment = segments.Last();

        var firstValue = GetElementValue(data[firstSegment.StartIndex], element);
        var lastValue = GetElementValue(data[lastSegment.EndIndex], element);

        if (!firstValue.HasValue || !lastValue.HasValue || firstValue.Value == 0)
            return null;

        // Calculate total drift percent
        var driftPercent = ((lastValue.Value - firstValue.Value) / firstValue.Value) * 100;

        // Calculate cumulative ratio across all segments (Python-compatible)
        decimal cumulativeRatio = 1.0m;
        foreach (var segment in segments)
        {
            if (segmentRatios.TryGetValue(segment.SegmentIndex, out var ratios) &&
                ratios.TryGetValue(element, out var ratio))
            {
                cumulativeRatio *= ratio;
            }
        }

        // Calculate average slope (for compatibility with existing DTOs)
        decimal totalSlope = 0;
        int validSegments = 0;
        foreach (var segment in segments)
        {
            var startVal = GetElementValue(data[segment.StartIndex], element);
            var endVal = GetElementValue(data[segment.EndIndex], element);
            if (startVal.HasValue && endVal.HasValue && segment.SampleCount > 0)
            {
                totalSlope += (endVal.Value - startVal.Value) / segment.SampleCount;
                validSegments++;
            }
        }
        var avgSlope = validSegments > 0 ? totalSlope / validSegments : 0;

        return new ElementDriftInfo(
            element,
            1.0m,                    // Initial ratio (normalized to 1)
            cumulativeRatio,         // Final ratio (cumulative across all segments)
            driftPercent,
            avgSlope,
            firstValue.Value         // Intercept = first value
        );
    }

    #endregion

    #region Private Helper Methods - Correction Algorithms (Python-Compatible)

    /// <summary>
    /// Apply linear correction using piecewise interpolation (Python-compatible)
    /// For each segment, interpolates correction factor linearly between start and end standards
    /// 
    /// Python equivalent (RM_check.py):
    ///   progress = (i - start) / (end - start)
    ///   effective_ratio = 1. 0 + (ratio - 1.0) * progress
    ///   correction = 1.0 / effective_ratio
    /// </summary>
    private List<CorrectedSampleDto> ApplyLinearCorrectionPiecewise(
        List<ParsedRow> data,
        List<DriftSegment> segments,
        List<string> elements,
        Dictionary<int, Dictionary<string, decimal>> segmentRatios)
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

                if (originalValue.HasValue && segment != null &&
                    segmentRatios.TryGetValue(segmentIndex, out var ratios) &&
                    ratios.TryGetValue(element, out var segmentRatio))
                {
                    // Python-compatible linear interpolation within segment
                    decimal factor = 1.0m;
                    var segmentLength = segment.EndIndex - segment.StartIndex;

                    if (segmentLength > 0)
                    {
                        // Python formula:
                        // progress = (i - start) / (end - start)
                        var progress = (decimal)(i - segment.StartIndex) / segmentLength;

                        // effective_ratio = 1. 0 + (ratio - 1.0) * progress
                        var effectiveRatio = 1.0m + (segmentRatio - 1.0m) * progress;

                        // correction = 1. 0 / effective_ratio
                        if (effectiveRatio != 0)
                            factor = 1.0m / effectiveRatio;
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

    /// <summary>
    /// Apply stepwise correction using arithmetic progression (Python-compatible)
    /// 
    /// Python formula (RM_check. py - ApplySingleRMThread):
    ///   delta = ratio - 1.0
    ///   step_delta = delta / n
    ///   effective_ratio = 1.0 + step_delta * (step_index + 1)
    /// 
    /// This is ARITHMETIC progression (linear addition), NOT GEOMETRIC (multiplication)
    /// </summary>
    private List<CorrectedSampleDto> ApplyStepwiseCorrectionArithmetic(
        List<ParsedRow> data,
        List<DriftSegment> segments,
        List<string> elements,
        Dictionary<int, Dictionary<string, decimal>> segmentRatios)
    {
        var result = new List<CorrectedSampleDto>();

        // Calculate total drift ratio and step delta for each element
        // Python: delta = ratio - 1.0; step_delta = delta / n
        var elementStepDeltas = new Dictionary<string, decimal>();

        foreach (var element in elements)
        {
            decimal totalRatio = 1.0m;

            // Calculate cumulative ratio across all segments
            foreach (var segment in segments)
            {
                if (segmentRatios.TryGetValue(segment.SegmentIndex, out var ratios) &&
                    ratios.TryGetValue(element, out var ratio))
                {
                    totalRatio *= ratio;
                }
            }

            // Python formula: delta = ratio - 1.0; step_delta = delta / n
            var delta = totalRatio - 1.0m;
            elementStepDeltas[element] = segments.Count > 0 ? delta / segments.Count : 0;
        }

        for (int i = 0; i < data.Count; i++)
        {
            var segment = segments.FirstOrDefault(s => i >= s.StartIndex && i <= s.EndIndex);
            var segmentIndex = segment?.SegmentIndex ?? 0;

            var correctedValues = new Dictionary<string, decimal?>();
            var correctionFactors = new Dictionary<string, decimal>();

            foreach (var element in elements)
            {
                var originalValue = GetElementValue(data[i], element);

                if (originalValue.HasValue && elementStepDeltas.TryGetValue(element, out var stepDelta))
                {
                    // Python formula: effective_ratio = 1.0 + step_delta * (step_index + 1)
                    // This is ARITHMETIC progression
                    var effectiveRatio = 1.0m + stepDelta * (segmentIndex + 1);
                    var factor = effectiveRatio != 0 ? 1.0m / effectiveRatio : 1.0m;

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

    /// <summary>
    /// Apply polynomial correction (2nd degree)
    /// Fits a quadratic curve to the data and normalizes to the mean
    /// </summary>
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

    #endregion

    #region Private Types

    private record ParsedRow(string SolutionLabel, Dictionary<string, decimal?> Values);

    #endregion
}