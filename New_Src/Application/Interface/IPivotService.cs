using Application.DTOs;
using Shared.Wrapper;

namespace Application.Services;

/// <summary>
/// Service for Pivot Table operations.  
/// Equivalent to pivot_tab.py and result. py in Python code.  
/// </summary>
public interface IPivotService
{
    /// <summary>
    /// Create pivot table from project raw data
    /// </summary>
    Task<Result<PivotResultDto>> GetPivotTableAsync(PivotRequest request);

    /// <summary>
    /// Create advanced pivot table with GCD/Repeat detection
    /// Equivalent to pivot_creator.py logic
    /// </summary>
    Task<Result<AdvancedPivotResultDto>> GetAdvancedPivotTableAsync(AdvancedPivotRequest request);

    /// <summary>
    /// Get all unique solution labels in project
    /// </summary>
    Task<Result<List<string>>> GetSolutionLabelsAsync(Guid projectId);

    /// <summary>
    /// Get all unique elements in project
    /// </summary>
    Task<Result<List<string>>> GetElementsAsync(Guid projectId);

    /// <summary>
    /// Detect duplicate rows based on patterns (TEK, RET, etc.)
    /// </summary>
    Task<Result<List<DuplicateResultDto>>> DetectDuplicatesAsync(DuplicateDetectionRequest request);

    /// <summary>
    /// Get column statistics (min, max, mean, std)
    /// </summary>
    Task<Result<Dictionary<string, ColumnStatsDto>>> GetColumnStatsAsync(Guid projectId);

    /// <summary>
    /// Export pivot table to CSV
    /// </summary>
    Task<Result<byte[]>> ExportToCsvAsync(PivotRequest request);

    /// <summary>
    /// Analyze project data for repeat patterns (GCD analysis)
    /// </summary>
    Task<Result<RepeatAnalysisDto>> AnalyzeRepeatsAsync(Guid projectId);
}

/// <summary>
/// Result of repeat pattern analysis
/// </summary>
public record RepeatAnalysisDto(
    bool HasRepeats,
    int TotalSamples,
    Dictionary<string, int> SetSizes,
    Dictionary<string, List<string>> RepeatedElements,
    Dictionary<string, int> ElementCounts
);