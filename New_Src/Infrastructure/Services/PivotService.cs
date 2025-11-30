using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Application.DTOs;
using Application.Services;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Wrapper;

namespace Infrastructure.Services;

/// <summary>
/// Implementation of IPivotService. 
/// Handles pivot table creation, filtering, and duplicate detection.
/// Equivalent to pivot_tab.py and result. py in Python code. 
/// </summary>
public class PivotService : IPivotService
{
    private readonly IsatisDbContext _db;
    private readonly ILogger<PivotService> _logger;

    // Default duplicate patterns from Python code
    private static readonly string[] DefaultDuplicatePatterns = { "TEK", "RET", "ret", "Ret" };

    public PivotService(IsatisDbContext db, ILogger<PivotService> logger)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<PivotResultDto>> GetPivotTableAsync(PivotRequest request)
    {
        try
        {
            // 1. Load project with raw data
            var project = await _db.Projects
                .AsNoTracking()
                .Include(p => p.RawDataRows)
                .FirstOrDefaultAsync(p => p.ProjectId == request.ProjectId);

            if (project == null)
                return Result<PivotResultDto>.Fail("Project not found");

            if (!project.RawDataRows.Any())
                return Result<PivotResultDto>.Fail("Project has no data");

            // 2. Parse raw data and build pivot structure
            var pivotData = new List<(string SolutionLabel, Dictionary<string, decimal?> Values, int Index)>();
            var allElements = new HashSet<string>();

            int index = 0;
            foreach (var rawRow in project.RawDataRows.OrderBy(r => r.DataId))
            {
                var rowData = ParseRowData(rawRow);
                if (rowData == null) continue;

                var solutionLabel = GetSolutionLabel(rawRow, rowData);
                if (string.IsNullOrWhiteSpace(solutionLabel)) continue;

                var values = new Dictionary<string, decimal?>();

                foreach (var kvp in rowData)
                {
                    if (IsMetadataColumn(kvp.Key)) continue;

                    var element = ExtractElementName(kvp.Key);
                    if (string.IsNullOrEmpty(element)) continue;

                    allElements.Add(kvp.Key);

                    decimal? value = ParseDecimalValue(kvp.Value);

                    // Apply oxide conversion if requested
                    if (request.UseOxide && value.HasValue)
                    {
                        var elementSymbol = ExtractElementSymbol(element);
                        if (OxideFactors.Factors.TryGetValue(elementSymbol, out var oxide))
                        {
                            value = value.Value * oxide.Factor;
                        }
                    }

                    values[kvp.Key] = value;
                }

                pivotData.Add((solutionLabel, values, index++));
            }

            // 3. Apply filters
            var filteredData = pivotData.AsEnumerable();

            // Filter by solution labels
            if (request.SelectedSolutionLabels?.Any() == true)
            {
                filteredData = filteredData.Where(d =>
                    request.SelectedSolutionLabels.Contains(d.SolutionLabel));
            }

            // Filter by search text
            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var search = request.SearchText.ToLower();
                filteredData = filteredData.Where(d =>
                    d.SolutionLabel.ToLower().Contains(search) ||
                    d.Values.Any(v => v.Value?.ToString().Contains(search) == true));
            }

            // Apply number filters
            if (request.NumberFilters?.Any() == true)
            {
                foreach (var filter in request.NumberFilters)
                {
                    var column = filter.Key;
                    var numberFilter = filter.Value;

                    filteredData = filteredData.Where(d =>
                    {
                        if (!d.Values.TryGetValue(column, out var val) || !val.HasValue)
                            return true; // Keep nulls

                        if (numberFilter.Min.HasValue && val.Value < numberFilter.Min.Value)
                            return false;
                        if (numberFilter.Max.HasValue && val.Value > numberFilter.Max.Value)
                            return false;

                        return true;
                    });
                }
            }

            var filteredList = filteredData.ToList();
            var totalCount = filteredList.Count;

            // 4.  Pagination
            var pagedData = filteredList
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            // 5. Filter columns if specific elements requested
            var columns = allElements.OrderBy(e => e).ToList();
            if (request.SelectedElements?.Any() == true)
            {
                columns = columns.Where(c =>
                    request.SelectedElements.Any(e => c.StartsWith(e))).ToList();
            }

            // 6. Build result rows
            var rows = pagedData.Select(d => new PivotRowDto(
                d.SolutionLabel,
                RoundValues(d.Values, request.DecimalPlaces),
                d.Index
            )).ToList();

            // 7. Calculate column stats
            var columnStats = CalculateColumnStats(filteredList, columns);

            // 8. Build metadata
            var metadata = new PivotMetadataDto(
                pivotData.Select(d => d.SolutionLabel).Distinct().OrderBy(s => s).ToList(),
                allElements.OrderBy(e => e).ToList(),
                columnStats
            );

            return Result<PivotResultDto>.Success(new PivotResultDto(
                columns,
                rows,
                totalCount,
                request.Page,
                request.PageSize,
                metadata
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get pivot table for project {ProjectId}", request.ProjectId);
            return Result<PivotResultDto>.Fail($"Failed to get pivot table: {ex.Message}");
        }
    }

    public async Task<Result<List<string>>> GetSolutionLabelsAsync(Guid projectId)
    {
        try
        {
            var project = await _db.Projects
                .AsNoTracking()
                .Include(p => p.RawDataRows)
                .FirstOrDefaultAsync(p => p.ProjectId == projectId);

            if (project == null)
                return Result<List<string>>.Fail("Project not found");

            var labels = new HashSet<string>();

            foreach (var rawRow in project.RawDataRows)
            {
                var rowData = ParseRowData(rawRow);
                if (rowData == null) continue;

                var label = GetSolutionLabel(rawRow, rowData);
                if (!string.IsNullOrWhiteSpace(label))
                    labels.Add(label);
            }

            return Result<List<string>>.Success(labels.OrderBy(l => l).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get solution labels for project {ProjectId}", projectId);
            return Result<List<string>>.Fail($"Failed to get solution labels: {ex.Message}");
        }
    }

    public async Task<Result<List<string>>> GetElementsAsync(Guid projectId)
    {
        try
        {
            var project = await _db.Projects
                .AsNoTracking()
                .Include(p => p.RawDataRows)
                .FirstOrDefaultAsync(p => p.ProjectId == projectId);

            if (project == null)
                return Result<List<string>>.Fail("Project not found");

            var elements = new HashSet<string>();

            foreach (var rawRow in project.RawDataRows)
            {
                var rowData = ParseRowData(rawRow);
                if (rowData == null) continue;

                foreach (var key in rowData.Keys)
                {
                    if (!IsMetadataColumn(key))
                    {
                        var element = ExtractElementName(key);
                        if (!string.IsNullOrEmpty(element))
                            elements.Add(key);
                    }
                }
            }

            return Result<List<string>>.Success(elements.OrderBy(e => e).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get elements for project {ProjectId}", projectId);
            return Result<List<string>>.Fail($"Failed to get elements: {ex.Message}");
        }
    }

    public async Task<Result<List<DuplicateResultDto>>> DetectDuplicatesAsync(DuplicateDetectionRequest request)
    {
        try
        {
            var pivotResult = await GetPivotTableAsync(new PivotRequest(request.ProjectId, PageSize: 10000));
            if (!pivotResult.Succeeded)
                return Result<List<DuplicateResultDto>>.Fail(pivotResult.Messages.FirstOrDefault() ?? "Failed to get pivot data");

            var pivotData = pivotResult.Data!;
            var patterns = request.DuplicatePatterns ?? DefaultDuplicatePatterns.ToList();
            var duplicates = new List<DuplicateResultDto>();

            // Build pattern regex
            var patternRegex = new Regex($@"\b({string.Join("|", patterns.Select(Regex.Escape))})\b", RegexOptions.IgnoreCase);

            // Find duplicate rows
            var duplicateRows = pivotData.Rows
                .Where(r => patternRegex.IsMatch(r.SolutionLabel))
                .ToList();

            foreach (var dupRow in duplicateRows)
            {
                // Extract base number from duplicate label (e.g., "123-TEK" -> "123")
                var baseNumber = ExtractBaseNumber(dupRow.SolutionLabel);
                if (string.IsNullOrEmpty(baseNumber)) continue;

                // Find main row
                var mainRow = pivotData.Rows
                    .FirstOrDefault(r => !patternRegex.IsMatch(r.SolutionLabel) &&
                                         r.SolutionLabel.Contains(baseNumber));

                if (mainRow == null) continue;

                // Calculate differences
                var differences = new List<ElementDiffDto>();
                bool hasOutOfRange = false;

                foreach (var col in pivotData.Columns)
                {
                    var mainVal = mainRow.Values.GetValueOrDefault(col);
                    var dupVal = dupRow.Values.GetValueOrDefault(col);

                    decimal? diffPercent = null;
                    bool isInRange = true;

                    if (mainVal.HasValue && dupVal.HasValue && mainVal.Value != 0)
                    {
                        diffPercent = Math.Abs((dupVal.Value - mainVal.Value) / mainVal.Value) * 100;
                        isInRange = diffPercent <= request.ThresholdPercent;
                        if (!isInRange) hasOutOfRange = true;
                    }

                    differences.Add(new ElementDiffDto(
                        col,
                        mainVal,
                        dupVal,
                        diffPercent.HasValue ? Math.Round(diffPercent.Value, 2) : null,
                        isInRange
                    ));
                }

                duplicates.Add(new DuplicateResultDto(
                    mainRow.SolutionLabel,
                    dupRow.SolutionLabel,
                    differences,
                    hasOutOfRange
                ));
            }

            return Result<List<DuplicateResultDto>>.Success(duplicates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to detect duplicates for project {ProjectId}", request.ProjectId);
            return Result<List<DuplicateResultDto>>.Fail($"Failed to detect duplicates: {ex.Message}");
        }
    }

    public async Task<Result<Dictionary<string, ColumnStatsDto>>> GetColumnStatsAsync(Guid projectId)
    {
        try
        {
            var pivotResult = await GetPivotTableAsync(new PivotRequest(projectId, PageSize: 10000));
            if (!pivotResult.Succeeded)
                return Result<Dictionary<string, ColumnStatsDto>>.Fail(pivotResult.Messages.FirstOrDefault() ?? "Failed");

            return Result<Dictionary<string, ColumnStatsDto>>.Success(pivotResult.Data!.Metadata.ColumnStats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get column stats for project {ProjectId}", projectId);
            return Result<Dictionary<string, ColumnStatsDto>>.Fail($"Failed: {ex.Message}");
        }
    }

    public async Task<Result<byte[]>> ExportToCsvAsync(PivotRequest request)
    {
        try
        {
            // Get all data (no pagination for export)
            var exportRequest = request with { Page = 1, PageSize = int.MaxValue };
            var pivotResult = await GetPivotTableAsync(exportRequest);

            if (!pivotResult.Succeeded)
                return Result<byte[]>.Fail(pivotResult.Messages.FirstOrDefault() ?? "Failed to get data");

            var pivot = pivotResult.Data!;
            var sb = new StringBuilder();

            // Header
            sb.AppendLine("Solution Label," + string.Join(",", pivot.Columns));

            // Rows
            foreach (var row in pivot.Rows)
            {
                var values = pivot.Columns.Select(c =>
                    row.Values.TryGetValue(c, out var v) && v.HasValue
                        ? v.Value.ToString()
                        : "");
                sb.AppendLine($"{row.SolutionLabel},{string.Join(",", values)}");
            }

            return Result<byte[]>.Success(Encoding.UTF8.GetBytes(sb.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export pivot to CSV for project {ProjectId}", request.ProjectId);
            return Result<byte[]>.Fail($"Failed to export: {ex.Message}");
        }
    }

    #region Private Helpers

    private Dictionary<string, object?>? ParseRowData(RawDataRow rawRow)
    {
        if (string.IsNullOrWhiteSpace(rawRow.ColumnData))
            return null;

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object?>>(rawRow.ColumnData);
        }
        catch
        {
            return null;
        }
    }

    private string? GetSolutionLabel(RawDataRow rawRow, Dictionary<string, object?> rowData)
    {
        if (!string.IsNullOrWhiteSpace(rawRow.SampleId))
            return rawRow.SampleId;

        if (rowData.TryGetValue("Solution Label", out var sl) && sl != null)
            return sl.ToString();

        if (rowData.TryGetValue("SolutionLabel", out var sl2) && sl2 != null)
            return sl2.ToString();

        return null;
    }

    private bool IsMetadataColumn(string columnName)
    {
        var metadataColumns = new[] {
            "Solution Label", "SolutionLabel", "SampleId", "Sample ID",
            "Type", "Act Wgt", "Act Vol", "DF", "Element", "Int", "Corr Con"
        };
        return metadataColumns.Contains(columnName, StringComparer.OrdinalIgnoreCase);
    }

    private string? ExtractElementName(string columnName)
    {
        if (string.IsNullOrWhiteSpace(columnName))
            return null;

        // Remove common suffixes like _1, _2, etc.
        var cleaned = Regex.Replace(columnName, @"_\d+$", "");
        return cleaned;
    }

    private string ExtractElementSymbol(string elementName)
    {
        var match = Regex.Match(elementName, @"^([A-Z][a-z]?)");
        return match.Success ? match.Groups[1].Value : elementName;
    }

    private string? ExtractBaseNumber(string label)
    {
        var match = Regex.Match(label, @"(\d+[-]?\d*)");
        return match.Success ? match.Groups[1].Value : null;
    }

    private decimal? ParseDecimalValue(object? value)
    {
        if (value == null) return null;

        if (value is JsonElement je)
        {
            if (je.ValueKind == JsonValueKind.Number && je.TryGetDecimal(out var d))
                return d;
            if (je.ValueKind == JsonValueKind.String && decimal.TryParse(je.GetString(), out var d2))
                return d2;
        }
        else if (decimal.TryParse(value.ToString(), out var d3))
        {
            return d3;
        }

        return null;
    }

    private Dictionary<string, decimal?> RoundValues(Dictionary<string, decimal?> values, int decimalPlaces)
    {
        return values.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.HasValue ? Math.Round(kvp.Value.Value, decimalPlaces) : (decimal?)null
        );
    }

    private Dictionary<string, ColumnStatsDto> CalculateColumnStats(
        List<(string SolutionLabel, Dictionary<string, decimal?> Values, int Index)> data,
        List<string> columns)
    {
        var stats = new Dictionary<string, ColumnStatsDto>();

        foreach (var col in columns)
        {
            var values = data
                .Select(d => d.Values.GetValueOrDefault(col))
                .Where(v => v.HasValue)
                .Select(v => v!.Value)
                .ToList();

            if (!values.Any())
            {
                stats[col] = new ColumnStatsDto(null, null, null, null, 0);
                continue;
            }

            var min = values.Min();
            var max = values.Max();
            var mean = values.Average();
            var count = values.Count;

            // Calculate standard deviation
            decimal? stdDev = null;
            if (count > 1)
            {
                var sumSquares = values.Sum(v => (v - (decimal)mean) * (v - (decimal)mean));
                stdDev = (decimal)Math.Sqrt((double)(sumSquares / (count - 1)));
            }

            stats[col] = new ColumnStatsDto(
                min,
                max,
                (decimal)Math.Round(mean, 4),
                stdDev.HasValue ? Math.Round(stdDev.Value, 4) : null,
                count
            );
        }

        return stats;
    }

    #endregion
}