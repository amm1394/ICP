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
/// Implementation of ICrmService. 
/// Handles CRM data management and difference calculations.
/// Equivalent to CRM. py and crm_manager.py in Python code.
/// </summary>
public class CrmService : ICrmService
{
    private readonly IsatisDbContext _db;
    private readonly ILogger<CrmService> _logger;

    // Default CRM patterns to search for (from Python code)
    private static readonly string[] DefaultCrmPatterns = { "258", "252", "906", "506", "233", "255", "263", "260" };

    public CrmService(IsatisDbContext db, ILogger<CrmService> logger)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<PaginatedResult<CrmListItemDto>>> GetCrmListAsync(
        string? analysisMethod = null,
        string? searchText = null,
        bool? ourOreasOnly = null,
        int page = 1,
        int pageSize = 50)
    {
        try
        {
            var query = _db.CrmData.AsNoTracking();

            // Filter by analysis method
            if (!string.IsNullOrWhiteSpace(analysisMethod) && analysisMethod != "All")
            {
                query = query.Where(c => c.AnalysisMethod == analysisMethod);
            }

            // Filter by Our OREAS
            if (ourOreasOnly == true)
            {
                query = query.Where(c => c.IsOurOreas);
            }

            // Search in CRM ID
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var search = searchText.ToLower();
                query = query.Where(c => c.CrmId.ToLower().Contains(search));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(c => c.CrmId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtos = items.Select(MapToDto).ToList();

            return Result<PaginatedResult<CrmListItemDto>>.Success(
                new PaginatedResult<CrmListItemDto>(dtos, totalCount, page, pageSize));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get CRM list");
            return Result<PaginatedResult<CrmListItemDto>>.Fail($"Failed to get CRM list: {ex.Message}");
        }
    }

    public async Task<Result<CrmListItemDto>> GetCrmByIdAsync(int id)
    {
        try
        {
            var crm = await _db.CrmData.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
            if (crm == null)
                return Result<CrmListItemDto>.Fail("CRM not found");

            return Result<CrmListItemDto>.Success(MapToDto(crm));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get CRM by ID {Id}", id);
            return Result<CrmListItemDto>.Fail($"Failed to get CRM: {ex.Message}");
        }
    }

    public async Task<Result<List<CrmListItemDto>>> GetCrmByCrmIdAsync(string crmId, string? analysisMethod = null)
    {
        try
        {
            var query = _db.CrmData.AsNoTracking()
                .Where(c => c.CrmId.Contains(crmId));

            if (!string.IsNullOrWhiteSpace(analysisMethod))
            {
                query = query.Where(c => c.AnalysisMethod == analysisMethod);
            }

            var items = await query.ToListAsync();
            var dtos = items.Select(MapToDto).ToList();

            return Result<List<CrmListItemDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get CRM by CrmId {CrmId}", crmId);
            return Result<List<CrmListItemDto>>.Fail($"Failed to get CRM: {ex.Message}");
        }
    }

    public async Task<Result<List<string>>> GetAnalysisMethodsAsync()
    {
        try
        {
            var methods = await _db.CrmData
                .AsNoTracking()
                .Where(c => c.AnalysisMethod != null)
                .Select(c => c.AnalysisMethod!)
                .Distinct()
                .OrderBy(m => m)
                .ToListAsync();

            return Result<List<string>>.Success(methods);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get analysis methods");
            return Result<List<string>>.Fail($"Failed to get analysis methods: {ex.Message}");
        }
    }

    /// <summary>
    /// Calculate differences between project data and CRM values. 
    /// Matches the logic in crm_manager.py: check_rm() and _build_crm_row_lists_for_columns()
    /// </summary>
    public async Task<Result<List<CrmDiffResultDto>>> CalculateDiffAsync(CrmDiffRequest request)
    {
        try
        {
            // 1. Load project raw data
            var project = await _db.Projects
                .AsNoTracking()
                .Include(p => p.RawDataRows)
                .FirstOrDefaultAsync(p => p.ProjectId == request.ProjectId);

            if (project == null)
                return Result<List<CrmDiffResultDto>>.Fail("Project not found");

            var patterns = request.CrmPatterns ?? DefaultCrmPatterns.ToList();
            var results = new List<CrmDiffResultDto>();

            // 2. Parse raw rows and find CRM matches
            foreach (var rawRow in project.RawDataRows)
            {
                if (string.IsNullOrWhiteSpace(rawRow.ColumnData))
                    continue;

                Dictionary<string, object?>? rowData;
                try
                {
                    rowData = JsonSerializer.Deserialize<Dictionary<string, object?>>(rawRow.ColumnData);
                }
                catch
                {
                    continue;
                }

                if (rowData == null)
                    continue;

                // Get Solution Label
                var solutionLabel = rawRow.SampleId ??
                    (rowData.TryGetValue("Solution Label", out var sl) ? sl?.ToString() : null) ??
                    (rowData.TryGetValue("SolutionLabel", out var sl2) ? sl2?.ToString() : null);

                if (string.IsNullOrWhiteSpace(solutionLabel))
                    continue;

                // Check if this row matches a CRM pattern
                var matchedCrmId = FindCrmMatch(solutionLabel, patterns);
                if (matchedCrmId == null)
                    continue;

                // 3. Find matching CRM in database
                var crmData = await _db.CrmData
                    .AsNoTracking()
                    .Where(c => c.CrmId.Contains(matchedCrmId))
                    .ToListAsync();

                if (!crmData.Any())
                    continue;

                // Prefer 4-Acid or Aqua Regia methods
                var preferredMethods = new[] { "4-Acid Digestion", "Aqua Regia Digestion" };
                var selectedCrm = crmData.FirstOrDefault(c => preferredMethods.Contains(c.AnalysisMethod))
                                  ?? crmData.First();

                // 4. Calculate differences
                var crmElements = ParseElementValues(selectedCrm.ElementValues);
                var differences = new List<ElementDiffDto>();

                foreach (var kvp in rowData)
                {
                    if (kvp.Key == "Solution Label" || kvp.Key == "SolutionLabel" || kvp.Key == "SampleId")
                        continue;

                    // Extract element symbol (e.g., "Fe 238. 204" -> "Fe")
                    var elementSymbol = ExtractElementSymbol(kvp.Key);
                    if (string.IsNullOrEmpty(elementSymbol))
                        continue;

                    // Get project value
                    decimal? projectValue = null;
                    if (kvp.Value is JsonElement je)
                    {
                        if (je.ValueKind == JsonValueKind.Number && je.TryGetDecimal(out var d))
                            projectValue = d;
                        else if (je.ValueKind == JsonValueKind.String && decimal.TryParse(je.GetString(), out var d2))
                            projectValue = d2;
                    }
                    else if (kvp.Value != null && decimal.TryParse(kvp.Value.ToString(), out var d3))
                    {
                        projectValue = d3;
                    }

                    // Get CRM value
                    decimal? crmValue = null;
                    if (crmElements.TryGetValue(elementSymbol, out var cv))
                        crmValue = cv;

                    // Calculate diff percent: ((crm - project) / crm) * 100
                    decimal? diffPercent = null;
                    bool isInRange = false;

                    if (projectValue.HasValue && crmValue.HasValue && crmValue.Value != 0)
                    {
                        diffPercent = ((crmValue.Value - projectValue.Value) / crmValue.Value) * 100;
                        isInRange = diffPercent >= request.MinDiffPercent && diffPercent <= request.MaxDiffPercent;
                    }

                    differences.Add(new ElementDiffDto(
                        kvp.Key,
                        projectValue,
                        crmValue,
                        diffPercent.HasValue ? Math.Round(diffPercent.Value, 2) : null,
                        isInRange
                    ));
                }

                if (differences.Any())
                {
                    results.Add(new CrmDiffResultDto(
                        solutionLabel,
                        selectedCrm.CrmId,
                        selectedCrm.AnalysisMethod ?? "Unknown",
                        differences
                    ));
                }
            }

            return Result<List<CrmDiffResultDto>>.Success(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to calculate CRM diff for project {ProjectId}", request.ProjectId);
            return Result<List<CrmDiffResultDto>>.Fail($"Failed to calculate diff: {ex.Message}");
        }
    }

    public async Task<Result<int>> UpsertCrmAsync(CrmUpsertRequest request)
    {
        try
        {
            var existing = await _db.CrmData
                .FirstOrDefaultAsync(c => c.CrmId == request.CrmId && c.AnalysisMethod == request.AnalysisMethod);

            var elementsJson = JsonSerializer.Serialize(request.Elements);

            if (existing != null)
            {
                existing.Type = request.Type;
                existing.ElementValues = elementsJson;
                existing.IsOurOreas = request.IsOurOreas;
                existing.UpdatedAt = DateTime.UtcNow;
                _db.CrmData.Update(existing);
            }
            else
            {
                var newCrm = new CrmData
                {
                    CrmId = request.CrmId,
                    AnalysisMethod = request.AnalysisMethod,
                    Type = request.Type,
                    ElementValues = elementsJson,
                    IsOurOreas = request.IsOurOreas,
                    CreatedAt = DateTime.UtcNow
                };
                _db.CrmData.Add(newCrm);
            }

            await _db.SaveChangesAsync();
            var id = existing?.Id ?? (await _db.CrmData.FirstAsync(c => c.CrmId == request.CrmId && c.AnalysisMethod == request.AnalysisMethod)).Id;

            return Result<int>.Success(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upsert CRM {CrmId}", request.CrmId);
            return Result<int>.Fail($"Failed to upsert CRM: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteCrmAsync(int id)
    {
        try
        {
            var crm = await _db.CrmData.FindAsync(id);
            if (crm == null)
                return Result<bool>.Fail("CRM not found");

            _db.CrmData.Remove(crm);
            await _db.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete CRM {Id}", id);
            return Result<bool>.Fail($"Failed to delete CRM: {ex.Message}");
        }
    }

    public async Task<Result<int>> ImportCrmsFromCsvAsync(Stream csvStream)
    {
        try
        {
            using var reader = new StreamReader(csvStream);
            var headerLine = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(headerLine))
                return Result<int>.Fail("CSV is empty");

            var headers = headerLine.Split(',').Select(h => h.Trim()).ToArray();
            var crmIdIndex = Array.FindIndex(headers, h => h.Equals("CRM ID", StringComparison.OrdinalIgnoreCase));
            var methodIndex = Array.FindIndex(headers, h => h.Equals("Analysis Method", StringComparison.OrdinalIgnoreCase));

            if (crmIdIndex < 0)
                return Result<int>.Fail("CSV must have 'CRM ID' column");

            var importedCount = 0;
            string? line;

            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var values = line.Split(',');
                if (values.Length <= crmIdIndex)
                    continue;

                var crmId = values[crmIdIndex].Trim();
                var method = methodIndex >= 0 && values.Length > methodIndex ? values[methodIndex].Trim() : null;

                // Parse element values
                var elements = new Dictionary<string, decimal>();
                for (int i = 0; i < headers.Length && i < values.Length; i++)
                {
                    if (i == crmIdIndex || i == methodIndex)
                        continue;

                    var header = headers[i];
                    if (decimal.TryParse(values[i], out var val))
                    {
                        var symbol = ExtractElementSymbol(header);
                        if (!string.IsNullOrEmpty(symbol))
                            elements[symbol] = val;
                    }
                }

                var request = new CrmUpsertRequest(crmId, method, null, elements, false);
                var result = await UpsertCrmAsync(request);
                if (result.Succeeded)
                    importedCount++;
            }

            return Result<int>.Success(importedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to import CRMs from CSV");
            return Result<int>.Fail($"Failed to import: {ex.Message}");
        }
    }

    #region Private Helpers

    private static CrmListItemDto MapToDto(CrmData crm)
    {
        var elements = ParseElementValues(crm.ElementValues);
        return new CrmListItemDto(
            crm.Id,
            crm.CrmId,
            crm.AnalysisMethod,
            crm.Type,
            crm.IsOurOreas,
            elements
        );
    }

    private static Dictionary<string, decimal> ParseElementValues(string json)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(json))
                return new Dictionary<string, decimal>();

            return JsonSerializer.Deserialize<Dictionary<string, decimal>>(json)
                   ?? new Dictionary<string, decimal>();
        }
        catch
        {
            return new Dictionary<string, decimal>();
        }
    }

    /// <summary>
    /// Find CRM match in solution label using patterns. 
    /// Matches logic from Python: is_crm_label()
    /// </summary>
    private static string? FindCrmMatch(string label, List<string> patterns)
    {
        if (string.IsNullOrWhiteSpace(label))
            return null;

        label = label.Trim();

        foreach (var pattern in patterns)
        {
            // Simple pattern: look for CRM/OREAS followed by the pattern number
            // Examples: "OREAS 258", "CRM258", "258a", "OREAS-258"
            var regexPattern = $@"(?:CRM|OREAS)? [\s\-]*({Regex.Escape(pattern)}[a-zA-Z0-9]{{0,2}})\b";

            try
            {
                var regex = new Regex(regexPattern, RegexOptions.IgnoreCase);
                var match = regex.Match(label);
                if (match.Success)
                {
                    return match.Groups[1].Value.Trim();
                }
            }
            catch
            {
                // If regex fails, try simple contains
                if (label.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                {
                    return pattern;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Extract element symbol from column name. 
    /// E.g., "Fe 238. 204" -> "Fe", "Cu_1" -> "Cu"
    /// </summary>
    private static string? ExtractElementSymbol(string columnName)
    {
        if (string.IsNullOrWhiteSpace(columnName))
            return null;

        // Match element symbols (1-2 letters, first uppercase)
        var match = Regex.Match(columnName, @"^([A-Z][a-z]?)");
        return match.Success ? match.Groups[1].Value : null;
    }

    #endregion
}