namespace Application.DTOs;

/// <summary>
/// Request for creating/getting pivot table
/// </summary>
public record PivotRequest(
    Guid ProjectId,
    string? SearchText = null,
    List<string>? SelectedSolutionLabels = null,
    List<string>? SelectedElements = null,
    Dictionary<string, NumberFilter>? NumberFilters = null,
    bool UseOxide = false,
    int DecimalPlaces = 2,
    int Page = 1,
    int PageSize = 100
);

/// <summary>
/// Number filter for numeric columns (min/max)
/// </summary>
public record NumberFilter(
    decimal? Min = null,
    decimal? Max = null
);

/// <summary>
/// Pivot table result
/// </summary>
public record PivotResultDto(
    List<string> Columns,
    List<PivotRowDto> Rows,
    int TotalCount,
    int Page,
    int PageSize,
    PivotMetadataDto Metadata
);

/// <summary>
/// Single row in pivot table
/// </summary>
public record PivotRowDto(
    string SolutionLabel,
    Dictionary<string, decimal?> Values,
    int OriginalIndex
);

/// <summary>
/// Metadata about the pivot table
/// </summary>
public record PivotMetadataDto(
    List<string> AllSolutionLabels,
    List<string> AllElements,
    Dictionary<string, ColumnStatsDto> ColumnStats
);

/// <summary>
/// Statistics for a column
/// </summary>
public record ColumnStatsDto(
    decimal? Min,
    decimal? Max,
    decimal? Mean,
    decimal? StdDev,
    int NonNullCount
);

/// <summary>
/// Duplicate detection request
/// </summary>
public record DuplicateDetectionRequest(
    Guid ProjectId,
    decimal ThresholdPercent = 10m,
    List<string>? DuplicatePatterns = null  // e.g., ["TEK", "RET", "ret"]
);

/// <summary>
/// Duplicate detection result
/// </summary>
public record DuplicateResultDto(
    string MainSolutionLabel,
    string DuplicateSolutionLabel,
    List<ElementDiffDto> Differences,
    bool HasOutOfRangeDiff
);

/// <summary>
/// Oxide conversion factors (from Python oxide_factors.py)
/// </summary>
public static class OxideFactors
{
    public static readonly Dictionary<string, (string Formula, decimal Factor)> Factors = new()
    {
        { "Al", ("Al2O3", 1.8895m) },
        { "Ba", ("BaO", 1.1165m) },
        { "Ca", ("CaO", 1.3992m) },
        { "Cr", ("Cr2O3", 1.4616m) },
        { "Fe", ("Fe2O3", 1.4297m) },
        { "K", ("K2O", 1.2046m) },
        { "Mg", ("MgO", 1.6583m) },
        { "Mn", ("MnO", 1.2912m) },
        { "Na", ("Na2O", 1.3480m) },
        { "P", ("P2O5", 2.2914m) },
        { "Si", ("SiO2", 2.1393m) },
        { "Ti", ("TiO2", 1.6681m) },
        { "S", ("SO3", 2.4972m) }
    };
}