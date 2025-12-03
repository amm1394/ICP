namespace Application.DTOs;

/// <summary>
/// Request for generating reports
/// </summary>
public record ReportRequest(
    Guid ProjectId,
    ReportType ReportType,
    ReportFormat Format = ReportFormat.Excel,
    ReportOptions? Options = null
);

/// <summary>
/// Report generation options
/// </summary>
public record ReportOptions(
    bool IncludeSummary = true,
    bool IncludeRawData = true,
    bool IncludeStatistics = true,
    bool IncludeRmCheck = true,
    bool IncludeDuplicates = true,
    bool UseOxide = false,
    int DecimalPlaces = 2,
    List<string>? SelectedElements = null,
    string? Title = null,
    string? Author = null
);

/// <summary>
/// Type of report
/// </summary>
public enum ReportType
{
    Full,           // Complete report with all data
    Summary,        // Summary statistics only
    RmCheck,        // RM check results only
    Duplicates,     // Duplicate analysis only
    PivotTable,     // Pivot table export
    CrmComparison   // CRM comparison report
}

/// <summary>
/// Report format
/// </summary>
public enum ReportFormat
{
    Excel,
    Csv,
    Json,
    Html
}

/// <summary>
/// Result of report generation
/// </summary>
public record ReportResultDto(
    string FileName,
    string ContentType,
    byte[] Data,
    DateTime GeneratedAt,
    ReportMetadataDto Metadata
);

/// <summary>
/// Report metadata
/// </summary>
public record ReportMetadataDto(
    Guid ProjectId,
    string ProjectName,
    ReportType ReportType,
    ReportFormat Format,
    int TotalRows,
    int TotalColumns,
    TimeSpan GenerationTime
);

/// <summary>
/// Export request for simple exports
/// </summary>
public record ExportRequest(
    Guid ProjectId,
    ReportFormat Format = ReportFormat.Excel,
    bool UseOxide = false,
    int DecimalPlaces = 2,
    List<string>? SelectedElements = null,
    List<string>? SelectedSolutionLabels = null
);

// ============================================
// Best Wavelength Selection DTOs
// Based on Python report.py select_best_wavelength_for_row()
// ============================================

/// <summary>
/// Calibration range for an element/wavelength
/// Calculated from Blk data Soln Conc values
/// </summary>
public record CalibrationRange(
    string Element,
    decimal Min,
    decimal Max,
    string DisplayRange  // "[min to max]" format
);

/// <summary>
/// Request for best wavelength selection
/// </summary>
public record BestWavelengthRequest(
    Guid ProjectId,
    List<string>? SelectedSolutionLabels = null,
    bool UseConcentration = true  // Use Soln Conc (true) or Corr Con (false)
);

/// <summary>
/// Result of best wavelength selection
/// </summary>
public record BestWavelengthResult(
    /// <summary>
    /// Calibration ranges per element: Element -> CalibrationRange
    /// </summary>
    Dictionary<string, CalibrationRange> CalibrationRanges,
    
    /// <summary>
    /// Best wavelength per row per base element
    /// Row Index -> (Base Element -> Best Wavelength Column)
    /// </summary>
    Dictionary<int, Dictionary<string, string>> BestWavelengthsPerRow,
    
    /// <summary>
    /// Base elements with their wavelength variants
    /// Base Element -> List of wavelength columns
    /// </summary>
    Dictionary<string, List<string>> BaseElements,
    
    /// <summary>
    /// Selected columns (Solution Label + best wavelengths)
    /// </summary>
    List<string> SelectedColumns
);

/// <summary>
/// Wavelength selection info for a single row
/// </summary>
public record WavelengthSelectionInfo(
    string SolutionLabel,
    string BaseElement,
    string SelectedWavelength,
    decimal? Concentration,
    bool IsInCalibrationRange,
    decimal? DistanceFromRange  // null if in range, otherwise distance to nearest bound
);