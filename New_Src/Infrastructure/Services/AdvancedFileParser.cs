using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Application.DTOs;
using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

/// <summary>
/// Advanced file parser supporting multiple ICP-MS file formats. 
/// Equivalent to load_file. py logic in Python code.
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
                return DetectExcelFormat(stream);
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
        string? line;
        int lineCount = 0;
        while ((line = await reader.ReadLineAsync()) != null && lineCount < 20)
        {
            previewLines.Add(line);
            lineCount++;
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

        // Check if second row is header
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

    private FileFormatDetectionResult DetectExcelFormat(Stream stream)
    {
        try
        {
            stream.Position = 0;
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.First();

            var previewRows = new List<List<string>>();
            var usedRange = worksheet.RangeUsed();
            if (usedRange == null)
            {
                return new FileFormatDetectionResult(FileFormat.Unknown, null, null, new List<string>(), "Excel file is empty");
            }

            // Read first 20 rows for detection
            int maxRows = Math.Min(20, usedRange.RowCount());
            int maxCols = usedRange.ColumnCount();

            for (int row = 1; row <= maxRows; row++)
            {
                var rowData = new List<string>();
                for (int col = 1; col <= maxCols; col++)
                {
                    var cell = worksheet.Cell(row, col);
                    rowData.Add(cell.GetString());
                }
                previewRows.Add(rowData);
            }

            // Check for Sample ID-based format
            bool hasSampleIdMarker = previewRows.Any(r => r.Any(c => SampleIdPattern.IsMatch(c)));

            if (hasSampleIdMarker)
            {
                return new FileFormatDetectionResult(
                    FileFormat.SampleIdBasedExcel,
                    null,
                    null,
                    new List<string> { "Solution Label", "Element", "Int", "Corr Con", "Type" },
                    "Detected Sample ID-based Excel format"
                );
            }

            // Check for tabular format - first row as header
            var firstRowHeaders = previewRows.FirstOrDefault() ?? new List<string>();
            bool hasRequiredColumns = firstRowHeaders.Any(h => SolutionLabelColumns.Contains(h, StringComparer.OrdinalIgnoreCase)) &&
                                       firstRowHeaders.Any(h => ElementColumns.Contains(h, StringComparer.OrdinalIgnoreCase));

            if (hasRequiredColumns)
            {
                return new FileFormatDetectionResult(
                    FileFormat.TabularExcel,
                    null,
                    0,
                    firstRowHeaders,
                    "Detected tabular Excel format"
                );
            }

            // Check second row as header
            if (previewRows.Count > 1)
            {
                var secondRowHeaders = previewRows[1];
                hasRequiredColumns = secondRowHeaders.Any(h => SolutionLabelColumns.Contains(h, StringComparer.OrdinalIgnoreCase)) &&
                                      secondRowHeaders.Any(h => ElementColumns.Contains(h, StringComparer.OrdinalIgnoreCase));

                if (hasRequiredColumns)
                {
                    return new FileFormatDetectionResult(
                        FileFormat.TabularExcel,
                        null,
                        1,
                        secondRowHeaders,
                        "Detected tabular Excel format (header on row 2)"
                    );
                }
            }

            return new FileFormatDetectionResult(
                FileFormat.TabularExcel,
                null,
                0,
                firstRowHeaders,
                "Assumed tabular Excel format"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to detect Excel format");
            return new FileFormatDetectionResult(FileFormat.Unknown, null, null, new List<string>(), $"Excel detection failed: {ex.Message}");
        }
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
                    (rows, warnings) = ParseTabularExcel(stream, options, progress, cancellationToken);
                    break;

                case FileFormat.SampleIdBasedExcel:
                    (rows, warnings) = ParseSampleIdBasedExcel(stream, options, progress, cancellationToken);
                    break;

                default:
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
    /// Parse Tabular Excel format using ClosedXML
    /// </summary>
    private (List<ParsedFileRow>, List<ImportWarning>) ParseTabularExcel(
        Stream stream,
        AdvancedImportRequest? options,
        IProgress<(int total, int processed, string message)>? progress,
        CancellationToken cancellationToken)
    {
        var rows = new List<ParsedFileRow>();
        var warnings = new List<ImportWarning>();

        try
        {
            stream.Position = 0;
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.First();

            var usedRange = worksheet.RangeUsed();
            if (usedRange == null)
            {
                warnings.Add(new ImportWarning(null, "", "Excel file is empty", ImportWarningLevel.Error));
                return (rows, warnings);
            }

            int totalRows = usedRange.RowCount();
            int totalCols = usedRange.ColumnCount();
            int headerRow = (options?.HeaderRow ?? 0) + 1; // ClosedXML is 1-based

            // Read headers
            var headers = new List<string>();
            for (int col = 1; col <= totalCols; col++)
            {
                headers.Add(worksheet.Cell(headerRow, col).GetString().Trim());
            }

            // Map columns
            var columnMap = MapColumns(headers.ToArray(), options?.ColumnMappings);

            int processedRows = 0;

            // Read data rows (skip header and optionally last row)
            int endRow = options?.SkipLastRow == true ? totalRows - 1 : totalRows;

            for (int row = headerRow + 1; row <= endRow; row++)
            {
                if (cancellationToken.IsCancellationRequested) break;

                try
                {
                    var solutionLabel = GetExcelColumnValue(worksheet, row, columnMap, "SolutionLabel", headers) ?? "Unknown";
                    var element = GetExcelColumnValue(worksheet, row, columnMap, "Element", headers) ?? "";
                    var intensityStr = GetExcelColumnValue(worksheet, row, columnMap, "Intensity", headers);
                    var corrConStr = GetExcelColumnValue(worksheet, row, columnMap, "CorrCon", headers);
                    var typeStr = GetExcelColumnValue(worksheet, row, columnMap, "Type", headers);

                    if (string.IsNullOrWhiteSpace(element)) continue;

                    element = NormalizeElementName(element);
                    var intensity = ParseDecimal(intensityStr);
                    var corrCon = ParseDecimal(corrConStr);
                    var type = !string.IsNullOrEmpty(typeStr) ? typeStr : DetectSampleType(solutionLabel, options);

                    var actWgtStr = GetExcelColumnValue(worksheet, row, columnMap, "ActWgt", headers);
                    var actVolStr = GetExcelColumnValue(worksheet, row, columnMap, "ActVol", headers);
                    var dfStr = GetExcelColumnValue(worksheet, row, columnMap, "DF", headers);

                    rows.Add(new ParsedFileRow(
                        solutionLabel,
                        element,
                        intensity,
                        corrCon,
                        type,
                        ParseDecimal(actWgtStr),
                        ParseDecimal(actVolStr),
                        ParseDecimal(dfStr),
                        new Dictionary<string, object?>()
                    ));
                }
                catch (Exception ex)
                {
                    warnings.Add(new ImportWarning(row, "", $"Failed to parse row: {ex.Message}", ImportWarningLevel.Warning));
                }

                processedRows++;
                if (processedRows % 100 == 0)
                {
                    progress?.Report((totalRows, processedRows, $"Parsing Excel row {processedRows}/{totalRows}"));
                }
            }

            _logger.LogInformation("Parsed {RowCount} rows from Excel file", rows.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse tabular Excel");
            warnings.Add(new ImportWarning(null, "", $"Excel parse error: {ex.Message}", ImportWarningLevel.Error));
        }

        return (rows, warnings);
    }

    /// <summary>
    /// Parse Sample ID-based Excel format
    /// </summary>
    private (List<ParsedFileRow>, List<ImportWarning>) ParseSampleIdBasedExcel(
        Stream stream,
        AdvancedImportRequest? options,
        IProgress<(int total, int processed, string message)>? progress,
        CancellationToken cancellationToken)
    {
        var rows = new List<ParsedFileRow>();
        var warnings = new List<ImportWarning>();

        try
        {
            stream.Position = 0;
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.First();

            var usedRange = worksheet.RangeUsed();
            if (usedRange == null)
            {
                warnings.Add(new ImportWarning(null, "", "Excel file is empty", ImportWarningLevel.Error));
                return (rows, warnings);
            }

            int totalRows = usedRange.RowCount();
            int totalCols = usedRange.ColumnCount();
            string? currentSample = null;
            int processedRows = 0;

            for (int row = 1; row <= totalRows; row++)
            {
                if (cancellationToken.IsCancellationRequested) break;

                // Skip last row if option set
                if (options?.SkipLastRow == true && row == totalRows) continue;

                var firstCellValue = worksheet.Cell(row, 1).GetString();

                // Check for Sample ID marker
                if (SampleIdPattern.IsMatch(firstCellValue))
                {
                    // Try to get sample ID from second column or parse from first
                    var secondCell = worksheet.Cell(row, 2).GetString();
                    if (!string.IsNullOrWhiteSpace(secondCell))
                    {
                        currentSample = secondCell.Trim();
                    }
                    else
                    {
                        // Parse from first cell: "Sample ID: XXX"
                        var match = Regex.Match(firstCellValue, @"Sample\s*ID\s*:\s*(.+)", RegexOptions.IgnoreCase);
                        currentSample = match.Success ? match.Groups[1].Value.Trim() : "Unknown";
                    }
                    continue;
                }

                // Skip metadata lines
                if (MethodFilePattern.IsMatch(firstCellValue) || CalibrationPattern.IsMatch(firstCellValue))
                {
                    continue;
                }

                // Parse data row
                if (currentSample != null && !string.IsNullOrWhiteSpace(firstCellValue))
                {
                    try
                    {
                        var element = NormalizeElementName(firstCellValue);
                        var intensityStr = worksheet.Cell(row, 2).GetString();
                        var corrConStr = totalCols >= 6 ? worksheet.Cell(row, 6).GetString() : null;

                        var intensity = ParseDecimal(intensityStr);
                        var corrCon = ParseDecimal(corrConStr);

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
                        warnings.Add(new ImportWarning(row, firstCellValue, $"Failed to parse: {ex.Message}", ImportWarningLevel.Warning));
                    }
                }

                processedRows++;
                if (processedRows % 100 == 0)
                {
                    progress?.Report((totalRows, processedRows, $"Parsing Excel row {processedRows}/{totalRows}"));
                }
            }

            _logger.LogInformation("Parsed {RowCount} rows from Sample ID-based Excel", rows.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse Sample ID-based Excel");
            warnings.Add(new ImportWarning(null, "", $"Excel parse error: {ex.Message}", ImportWarningLevel.Error));
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
        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            allLines.Add(line);
        }

        int totalRows = allLines.Count;
        string? currentSample = null;
        int processedRows = 0;

        for (int i = 0; i < allLines.Count; i++)
        {
            if (cancellationToken.IsCancellationRequested) break;

            line = allLines[i];

            if (string.IsNullOrWhiteSpace(line)) continue;
            if (options?.SkipLastRow == true && i == allLines.Count - 1) continue;

            var parts = ParseCsvLine(line);

            if (parts.Length > 0 && SampleIdPattern.IsMatch(parts[0]))
            {
                currentSample = parts.Length > 1 ? parts[1].Trim() : "Unknown";
                continue;
            }

            if (parts.Length > 0 && (MethodFilePattern.IsMatch(parts[0]) || CalibrationPattern.IsMatch(parts[0])))
            {
                continue;
            }

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

        int headerRow = options?.HeaderRow ?? 0;
        for (int i = 0; i < headerRow && csv.Read(); i++) { }

        if (!csv.Read())
        {
            warnings.Add(new ImportWarning(null, "", "File is empty or has no header", ImportWarningLevel.Error));
            return (rows, warnings);
        }

        csv.ReadHeader();
        var headers = csv.HeaderRecord ?? Array.Empty<string>();
        var columnMap = MapColumns(headers, options?.ColumnMappings);

        int rowNumber = headerRow + 1;
        int totalEstimate = 1000;
        int processedRows = 0;

        while (csv.Read())
        {
            if (cancellationToken.IsCancellationRequested) break;

            rowNumber++;

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
                    new Dictionary<string, object?>()
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

    private string DetectSampleType(string solutionLabel, AdvancedImportRequest? options)
    {
        if (!(options?.AutoDetectType ?? true))
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

    private Dictionary<string, int> MapColumns(string[] headers, Dictionary<string, string>? customMappings)
    {
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < headers.Length; i++)
        {
            var header = headers[i].Trim();

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

    private string? GetExcelColumnValue(IXLWorksheet worksheet, int row, Dictionary<string, int> columnMap, string columnName, List<string> headers)
    {
        if (columnMap.TryGetValue(columnName, out var index))
        {
            try
            {
                return worksheet.Cell(row, index + 1).GetString(); // ClosedXML is 1-based
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