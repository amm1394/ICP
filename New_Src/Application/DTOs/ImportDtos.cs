namespace Application.DTOs;

/// <summary>
/// File format detection result
/// </summary>
public enum FileFormat
{
    Unknown,
    TabularCsv,           // Standard CSV with headers: Solution Label, Element, Int, Corr Con
    TabularExcel,         // Standard Excel with same structure
    SampleIdBasedCsv,     // New format: "Sample ID:" markers
    SampleIdBasedExcel,   // New format Excel
    IcpMassHunter,        // Agilent ICP-MS MassHunter export
    PerkinElmer           // PerkinElmer Syngistix export
}

/// <summary>
/// Result of file format detection
/// </summary>
public record FileFormatDetectionResult(
    FileFormat Format,
    string? DetectedDelimiter,
    int? HeaderRowIndex,
    List<string> DetectedColumns,
    string? Message
);

/// <summary>
/// Advanced import request with options
/// </summary>
public record AdvancedImportRequest(
    string ProjectName,
    string? Owner = null,
    FileFormat? ForceFormat = null,
    string? Delimiter = null,
    int? HeaderRow = null,
    Dictionary<string, string>? ColumnMappings = null,  // Map file columns to expected columns
    bool SkipLastRow = true,
    bool AutoDetectType = true,  // Auto-detect Samp/Blk/Std from Solution Label
    string? DefaultType = "Samp"
);

/// <summary>
/// Import result with detailed info
/// </summary>
public record AdvancedImportResult(
    Guid ProjectId,
    int TotalRowsRead,
    int TotalRowsImported,
    int SkippedRows,
    FileFormat DetectedFormat,
    List<string> ImportedSolutionLabels,
    List<string> ImportedElements,
    List<ImportWarning> Warnings
);

/// <summary>
/// Warning during import
/// </summary>
public record ImportWarning(
    int? RowNumber,
    string Column,
    string Message,
    ImportWarningLevel Level
);

/// <summary>
/// Warning severity level
/// </summary>
public enum ImportWarningLevel
{
    Info,
    Warning,
    Error
}

/// <summary>
/// Parsed row from file
/// </summary>
public record ParsedFileRow(
    string SolutionLabel,
    string Element,
    decimal? Intensity,
    decimal? CorrCon,
    string Type,
    decimal? ActWgt,
    decimal? ActVol,
    decimal? DF,
    Dictionary<string, object?> AdditionalColumns
);

/// <summary>
/// Preview result for file before import
/// </summary>
public record FilePreviewResult(
    FileFormat DetectedFormat,
    List<string> Headers,
    List<Dictionary<string, string>> PreviewRows,
    int TotalRows,
    List<string> SuggestedColumnMappings,
    string? Message
);