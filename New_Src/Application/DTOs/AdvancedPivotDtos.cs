namespace Application.DTOs;

/// <summary>
/// Request for advanced pivot table with GCD/Repeat support
/// </summary>
public record AdvancedPivotRequest(
    Guid ProjectId,
    string? SearchText = null,
    List<string>? SelectedSolutionLabels = null,
    List<string>? SelectedElements = null,
    Dictionary<string, NumberFilter>? NumberFilters = null,
    bool UseOxide = false,
    bool UseInt = false,  // Use 'Int' column instead of 'Corr Con'
    int DecimalPlaces = 2,
    int Page = 1,
    int PageSize = 100,
    PivotAggregation Aggregation = PivotAggregation.First,
    bool MergeRepeats = false  // If true, average repeated elements instead of creating _1, _2
);

/// <summary>
/// Aggregation function for pivot
/// </summary>
public enum PivotAggregation
{
    First,
    Last,
    Mean,
    Sum,
    Min,
    Max,
    Count
}

/// <summary>
/// Advanced pivot result with repeat info
/// </summary>
public record AdvancedPivotResultDto(
    List<string> Columns,
    List<AdvancedPivotRowDto> Rows,
    int TotalCount,
    int Page,
    int PageSize,
    AdvancedPivotMetadataDto Metadata
);

/// <summary>
/// Row in advanced pivot with set info
/// </summary>
public record AdvancedPivotRowDto(
    string SolutionLabel,
    Dictionary<string, decimal?> Values,
    int OriginalIndex,
    int SetIndex,      // Which set (0, 1, 2, .. .) for repeated samples
    int SetSize        // Total sets for this solution label
);

/// <summary>
/// Metadata with repeat detection info
/// </summary>
public record AdvancedPivotMetadataDto(
    List<string> AllSolutionLabels,
    List<string> AllElements,
    Dictionary<string, ColumnStatsDto> ColumnStats,
    bool HasRepeats,
    Dictionary<string, int> SetSizes,  // Solution Label -> set size (from GCD)
    Dictionary<string, List<string>> RepeatedElements  // Elements that repeat within sets
);

// NOTE: ElementDiffDto is already defined in CrmDtos.cs - no need to redefine here