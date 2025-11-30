using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Application.DTOs;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

/// <summary>
/// Advanced file parser supporting multiple ICP-MS file formats. 
/// Equivalent to load_file.py logic in Python code.
/// </summary>
public class AdvancedFileParser
{
    private readonly ILogger _logger;

    // Patterns for format detection
    private static readonly Regex SampleIdPattern = new(@"^Sample\s*ID\s*:", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex MethodFilePattern = new(@"^Method\s*File\s*:", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex CalibrationPattern = new(@"^Calibration\s*File\s*:", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex ElementPattern = new(@"^([A-Za-z]{1,2})(\d+\. ?\d*)$", RegexOptions.Compiled);

    // Known column names
    private static readonly string[] SolutionLabelColumns = { "Solution Label", "SolutionLabel", "Sample ID", "SampleId", "Sample" };
    private static readonly string[] ElementColumns = { "Element", "Analyte", "Mass" };
    private static readonly string[] IntensityColumns = { "Int", "Intensity", "Net Intensity", "CPS", "Counts" };
    private static readonly string[] ConcentrationColumns = { "Corr Con", "CorrCon", "Concentration", "Conc", "Calibrated Conc" };
    private static readonly string[] TypeColumns = { "Type", "Sample Type", "SampleType" };

    public AdvancedFileParser(ILogger logger)
    {
        _logger = logger;
    }

    #region Format Detection

    /// <summary>
    /// Detect file format from stream
    /// </summary>
    public async Task<FileFormatDetectionResult> DetectFormatAsync(Stream stream, string fileName)
    {
        try
        {
            var isExcel = fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase) ||
                          fileName.EndsWith(". xls", StringComparison.OrdinalIgnoreCase);

            if (isExcel)
            {
                return await DetectExcelFormatAsync(stream);
            }
            else
            {
                return await DetectCsvFormatAsync(stream);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to detect file format for {FileName}", fileName);
            return new FileFormatDetectionResult(
                FileFormat.Unknown,
                null,
                null,
                new List<string>(),
                $"Failed to detect format: {ex.Message}"
            );
        }
    }

    private async Task<FileFormatDetectionResult> DetectCsvFormatAsync(Stream stream)
    {
        stream.Position = 0;
        using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);

        var previewLines = new List<string>();
        for (int i = 0; i < 20 && !reader.EndOfStream; i++)
        {
            var line = await reader.ReadLineAsync();
            if (line != null) previewLines.Add(line);
        }

        stream.Position = 0;

        if (!previewLines.Any())
        {
            return new FileFormatDetectionResult(FileFormat.Unknown, null, null, new List<string>(), "File is empty");
        }

        // Check for Sample ID-based format
        bool hasSampleIdMarker = previewLines.Any(l => SampleIdPattern.IsMatch(l));
        bool hasNetIntensity = previewLines.Any(l => l.Contains("Net Intensity", StringComparison.OrdinalIgnoreCase));

        if (hasSampleIdMarker || hasNetIntensity)
        {
            return new FileFormatDetectionResult(
                FileFormat.SampleIdBasedCsv,
                ",",
                null,
                new List<string> { "Solution Label", "Element", "Int", "Corr Con", "Type" },
                "Detected Sample ID-based format (ICP-MS export)"
            );
        }

        // Detect delimiter
        var delimiter = DetectDelimiter(previewLines.First());

        // Check for tabular format
        var firstLine = previewLines.First();
        var headers = firstLine.Split(new[] { delimiter }, StringSplitOptions.None)
            .Select(h => h.Trim().Trim('"'))
            .ToList();

        bool hasRequiredColumns = headers.Any(h => SolutionLabelColumns.Contains(h, StringComparer.OrdinalIgnoreCase)) &&
                                   headers.Any(h => ElementColumns.Contains(h, StringComparer.OrdinalIgnoreCase));

        if (hasRequiredColumns)
        {
            return new FileFormatDetectionResult(
                FileFormat.TabularCsv,
                delimiter.ToString(),
                0,
                headers,
                "Detected tabular CSV format"
            );
        }

        // Check if second row is header (first row might be title)
        if (previewLines.Count > 1)
        {
            var secondLine = previewLines[1];
            var secondHeaders = secondLine.Split(new[] { delimiter }, StringSplitOptions.None)
                .Select(h => h.Trim().Trim('"'))
                .ToList();

            hasRequiredColumns = secondHeaders.Any(h => SolutionLabelColumns.Contains(h, StringComparer.OrdinalIgnoreCase)) &&
                                  secondHeaders.Any(h => ElementColumns.Contains(h, StringComparer.OrdinalIgnoreCase));

            if (hasRequiredColumns)
            {
                return new FileFormatDetectionResult(
                    FileFormat.TabularCsv,
                    delimiter.ToString(),
                    1,
                    secondHeaders,
                    "Detected tabular CSV format (header on row 2)"
                );
            }
        }

        return new FileFormatDetectionResult(
            FileFormat.Unknown,
            delimiter.ToString(),
            null,
            headers,
            "Could not determine file format"
        );
    }

    private async Task<FileFormatDetectionResult> DetectExcelFormatAsync(Stream stream)
    {
        // For Excel, we'd need to use a library like EPPlus or ClosedXML
        // For now, return a placeholder
        return new FileFormatDetectionResult(
            FileFormat.TabularExcel,
            null,
            0,
            new List<string>(),
            "Excel format detected - will parse on import"
        );
    }

    private char DetectDelimiter(string line)
    {
        var delimiters = new[] { ',', ';', '\t', '|' };
        var counts = delimiters.ToDictionary(d => d, d => line.Count(c => c == d));
        return counts.OrderByDescending(kv => kv.Value).First().Key;
    }

    #endregion

    #region Parsing

    /// <summary>
    /// Parse file and return rows
    /// </summary>
    public async Task<(List<ParsedFileRow> Rows, List<ImportWarning> Warnings)> ParseFileAsync(
        Stream stream,
        string fileName,
        FileFormat format,
        AdvancedImportRequest? options = null,
        IProgress<(int total, int processed, string message)>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var rows = new List<ParsedFileRow>();
        var warnings = new List<ImportWarning>();

        try
        {
            switch (format)
            {
                case FileFormat.SampleIdBasedCsv:
                    (rows, warnings) = await ParseSampleIdBasedCsvAsync(stream, options, progress, cancellationToken);
                    break;

                case FileFormat.TabularCsv:
                    (rows, warnings) = await ParseTabularCsvAsync(stream, options, progress, cancellationToken);
                    break;

                case FileFormat.TabularExcel:
                case FileFormat.SampleIdBasedExcel:
                    (rows, warnings) = await ParseExcelAsync(stream, fileName, format, options, progress, cancellationToken);
                    break;

                default:
                    // Try auto-detection
                    var detection = await DetectFormatAsync(stream, fileName);
                    if (detection.Format != FileFormat.Unknown)
                    {
                        stream.Position = 0;
                        return await ParseFileAsync(stream, fileName, detection.Format, options, progress, cancellationToken);
                    }
                    warnings.Add(new ImportWarning(null, "", "Unknown file format", ImportWarningLevel.Error));
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse file {FileName}", fileName);
            warnings.Add(new ImportWarning(null, "", $"Parse error: {ex.Message}", ImportWarningLevel.Error));
        }

        return (rows, warnings);
    }

    /// <summary>
    /// Parse Sample ID-based format (ICP-MS export)
    /// </summary>
    private async Task<(List<ParsedFileRow>, List<ImportWarning>)> ParseSampleIdBasedCsvAsync(
        Stream stream,
        AdvancedImportRequest? options,
        IProgress<(int total, int processed, string message)>? progress,
        CancellationToken cancellationToken)
    {
        var rows = new List<ParsedFileRow>();
        var warnings = new List<ImportWarning>();

        stream.Position = 0;
        using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);

        var allLines = new List<string>();
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (line != null) allLines.Add(line);
        }

        int totalRows = allLines.Count;
        string? currentSample = null;
        int processedRows = 0;

        for (int i = 0; i < allLines.Count; i++)
        {
            if (cancellationToken.IsCancellationRequested) break;

            var line = allLines[i];

            // Skip empty lines
            if (string.IsNullOrWhiteSpace(line)) continue;

            // Skip last row if option set
            if (options?.SkipLastRow == true && i == allLines.Count - 1) continue;

            var parts = ParseCsvLine(line);

            // Check for Sample ID marker
            if (parts.Length > 0 && SampleIdPattern.IsMatch(parts[0]))
            {
                currentSample = parts.Length > 1 ? parts[1].Trim() : "Unknown";
                continue;
            }

            // Skip metadata lines
            if (parts.Length > 0 && (MethodFilePattern.IsMatch(parts[0]) || CalibrationPattern.IsMatch(parts[0])))
            {
                continue;
            }

            // Parse data row
            if (currentSample != null && parts.Length > 0 && !string.IsNullOrWhiteSpace(parts[0]))
            {
                try
                {
                    var element = NormalizeElementName(parts[0]);
                    decimal? intensity = parts.Length > 1 ? ParseDecimal(parts[1]) : null;
                    decimal? corrCon = parts.Length > 5 ? ParseDecimal(parts[5]) : null;

                    if (intensity.HasValue || corrCon.HasValue)
                    {
                        var type = DetectSampleType(currentSample, options);

                        rows.Add(new ParsedFileRow(
                            currentSample,
                            element,
                            intensity,
                            corrCon,
                            type,
                            null, null, null,
                            new Dictionary<string, object?>()
                        ));
                    }
                }
                catch (Exception ex)
                {
                    warnings.Add(new ImportWarning(i + 1, parts[0], $"Failed to parse: {ex.Message}", ImportWarningLevel.Warning));
                }
            }

            processedRows++;
            if (processedRows % 100 == 0)
            {
                progress?.Report((totalRows, processedRows, $"Parsing row {processedRows}/{totalRows}"));
            }
        }

        return (rows, warnings);
    }

    /// <summary>
    /// Parse tabular CSV format
    /// </summary>
    private async Task<(List<ParsedFileRow>, List<ImportWarning>)> ParseTabularCsvAsync(
        Stream stream,
        AdvancedImportRequest? options,
        IProgress<(int total, int processed, string message)>? progress,
        CancellationToken cancellationToken)
    {
        var rows = new List<ParsedFileRow>();
        var warnings = new List<ImportWarning>();

        stream.Position = 0;

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            DetectDelimiter = true,
            BadDataFound = null,
            MissingFieldFound = null,
            TrimOptions = TrimOptions.Trim,
            HeaderValidated = null
        };

        using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
        using var csv = new CsvReader(reader, config);

        // Skip to header row if specified
        int headerRow = options?.HeaderRow ?? 0;
        for (int i = 0; i < headerRow && csv.Read(); i++) { }

        if (!csv.Read())
        {
            warnings.Add(new ImportWarning(null, "", "File is empty or has no header", ImportWarningLevel.Error));
            return (rows, warnings);
        }

        csv.ReadHeader();
        var headers = csv.HeaderRecord ?? Array.Empty<string>();

        // Find column indices
        var columnMap = MapColumns(headers, options?.ColumnMappings);

        int rowNumber = headerRow + 1;
        int totalEstimate = 1000; // Will update if we know total
        int processedRows = 0;

        while (csv.Read())
        {
            if (cancellationToken.IsCancellationRequested) break;

            rowNumber++;

            // Skip last row if option set
            if (options?.SkipLastRow == true && csv.Parser.RawRow == csv.Parser.Count - 1)
                continue;

            try
            {
                var solutionLabel = GetColumnValue(csv, columnMap, "SolutionLabel") ?? "Unknown";
                var element = GetColumnValue(csv, columnMap, "Element") ?? "";
                var intensityStr = GetColumnValue(csv, columnMap, "Intensity");
                var corrConStr = GetColumnValue(csv, columnMap, "CorrCon");
                var typeStr = GetColumnValue(csv, columnMap, "Type");

                if (string.IsNullOrWhiteSpace(element)) continue;

                element = NormalizeElementName(element);
                var intensity = ParseDecimal(intensityStr);
                var corrCon = ParseDecimal(corrConStr);
                var type = !string.IsNullOrEmpty(typeStr) ? typeStr : DetectSampleType(solutionLabel, options);

                // Parse additional columns
                var additional = new Dictionary<string, object?>();
                var actWgtStr = GetColumnValue(csv, columnMap, "ActWgt");
                var actVolStr = GetColumnValue(csv, columnMap, "ActVol");
                var dfStr = GetColumnValue(csv, columnMap, "DF");

                rows.Add(new ParsedFileRow(
                    solutionLabel,
                    element,
                    intensity,
                    corrCon,
                    type,
                    ParseDecimal(actWgtStr),
                    ParseDecimal(actVolStr),
                    ParseDecimal(dfStr),
                    additional
                ));
            }
            catch (Exception ex)
            {
                warnings.Add(new ImportWarning(rowNumber, "", $"Failed to parse row: {ex.Message}", ImportWarningLevel.Warning));
            }

            processedRows++;
            if (processedRows % 100 == 0)
            {
                progress?.Report((totalEstimate, processedRows, $"Parsing row {processedRows}"));
            }
        }

        return (rows, warnings);
    }

    /// <summary>
    /// Parse Excel file
    /// </summary>
    private async Task<(List<ParsedFileRow>, List<ImportWarning>)> ParseExcelAsync(
        Stream stream,
        string fileName,
        FileFormat format,
        AdvancedImportRequest? options,
        IProgress<(int total, int processed, string message)>? progress,
        CancellationToken cancellationToken)
    {
        // For Excel parsing, we'd need EPPlus or ClosedXML
        // This is a placeholder - actual implementation would use those libraries
        var warnings = new List<ImportWarning>
        {
            new ImportWarning(null, "", "Excel parsing requires additional setup.  Please export to CSV.", ImportWarningLevel. Warning)
        };

        return (new List<ParsedFileRow>(), warnings);
    }

    #endregion

    #region Helper Methods

    private string[] ParseCsvLine(string line)
    {
        var result = new List<string>();
        bool inQuotes = false;
        var current = new StringBuilder();

        foreach (char c in line)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(current.ToString().Trim());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        result.Add(current.ToString().Trim());
        return result.ToArray();
    }

    /// <summary>
    /// Normalize element name: "Ce140" -> "Ce 140"
    /// </summary>
    private string NormalizeElementName(string element)
    {
        if (string.IsNullOrWhiteSpace(element)) return element;

        element = element.Trim();
        var match = ElementPattern.Match(element);
        if (match.Success)
        {
            return $"{match.Groups[1].Value} {match.Groups[2].Value}";
        }

        return element;
    }

    /// <summary>
    /// Detect sample type from label
    /// </summary>
    private string DetectSampleType(string solutionLabel, AdvancedImportRequest? options)
    {
        if (!options?.AutoDetectType ?? false)
            return options?.DefaultType ?? "Samp";

        var upper = solutionLabel.ToUpperInvariant();

        if (upper.Contains("BLANK") || upper.Contains("BLK"))
            return "Blk";

        if (upper.Contains("STD") || upper.Contains("STANDARD") || upper.Contains("CAL"))
            return "Std";

        if (upper.Contains("QC") || upper.Contains("CHECK"))
            return "QC";

        if (Regex.IsMatch(upper, @"\b(OREAS|SRM|CRM)\b"))
            return "RM";

        return "Samp";
    }

    private decimal? ParseDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        value = value.Trim();
        if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            return result;
        return null;
    }

    /// <summary>
    /// Map file columns to expected columns
    /// </summary>
    private Dictionary<string, int> MapColumns(string[] headers, Dictionary<string, string>? customMappings)
    {
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < headers.Length; i++)
        {
            var header = headers[i].Trim();

            // Check custom mappings first
            if (customMappings != null)
            {
                foreach (var mapping in customMappings)
                {
                    if (header.Equals(mapping.Value, StringComparison.OrdinalIgnoreCase))
                    {
                        map[mapping.Key] = i;
                    }
                }
            }

            // Auto-map known columns
            if (SolutionLabelColumns.Contains(header, StringComparer.OrdinalIgnoreCase))
                map["SolutionLabel"] = i;
            else if (ElementColumns.Contains(header, StringComparer.OrdinalIgnoreCase))
                map["Element"] = i;
            else if (IntensityColumns.Contains(header, StringComparer.OrdinalIgnoreCase))
                map["Intensity"] = i;
            else if (ConcentrationColumns.Contains(header, StringComparer.OrdinalIgnoreCase))
                map["CorrCon"] = i;
            else if (TypeColumns.Contains(header, StringComparer.OrdinalIgnoreCase))
                map["Type"] = i;
            else if (header.Equals("Act Wgt", StringComparison.OrdinalIgnoreCase) ||
                     header.Equals("ActWgt", StringComparison.OrdinalIgnoreCase))
                map["ActWgt"] = i;
            else if (header.Equals("Act Vol", StringComparison.OrdinalIgnoreCase) ||
                     header.Equals("ActVol", StringComparison.OrdinalIgnoreCase))
                map["ActVol"] = i;
            else if (header.Equals("DF", StringComparison.OrdinalIgnoreCase))
                map["DF"] = i;
        }

        return map;
    }

    private string? GetColumnValue(CsvReader csv, Dictionary<string, int> columnMap, string columnName)
    {
        if (columnMap.TryGetValue(columnName, out var index))
        {
            try
            {
                return csv.GetField(index);
            }
            catch
            {
                return null;
            }
        }
        return null;
    }

    #endregion
}