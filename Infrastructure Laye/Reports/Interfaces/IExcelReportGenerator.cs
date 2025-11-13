using Infrastructure.Icp.Reports.Models;
using Infrastructure.Icp.Reports.Models.Configurations;
using Shared.Icp.DTOs.Reports;

namespace Infrastructure.Icp.Reports.Interfaces;

/// <summary>
/// رابط تولید گزارش Excel
/// </summary>
public interface IExcelReportGenerator : IReportGenerator
{
    /// <summary>
    /// تولید Excel با چندین Sheet
    /// </summary>
    Task<byte[]> GenerateMultiSheetExcelAsync<TData>(
        Dictionary<string, TData> sheetsData,
        string templateName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// افزودن Chart به Excel
    /// </summary>
    Task<byte[]> AddChartToExcelAsync<TData>(
        TData data,
        string templateName,
        ChartConfiguration chartConfig,
        string? sheetName = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// تولید Excel با فرمول‌ها
    /// </summary>
    Task<byte[]> GenerateExcelWithFormulasAsync<TData>(
        TData data,
        string templateName,
        Dictionary<string, string> formulas,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// اعمال فرمت‌بندی سفارشی
    /// </summary>
    Task<byte[]> ApplyCustomFormattingAsync(
        byte[] excelContent,
        ExcelFormattingOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// تولید Pivot Table
    /// </summary>
    Task<byte[]> AddPivotTableAsync<TData>(
        TData data,
        string templateName,
        PivotTableConfiguration pivotConfig,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// افزودن Conditional Formatting
    /// </summary>
    Task<byte[]> AddConditionalFormattingAsync(
        byte[] excelContent,
        string sheetName,
        string range,
        ConditionalFormattingRule rule,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// افزودن Data Validation
    /// </summary>
    Task<byte[]> AddDataValidationAsync(
        byte[] excelContent,
        string sheetName,
        string range,
        DataValidationRule validationRule,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// ادغام سلول‌ها
    /// </summary>
    Task<byte[]> MergeCellsAsync(
        byte[] excelContent,
        string sheetName,
        string range,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// افزودن تصویر به Excel
    /// </summary>
    Task<byte[]> AddImageAsync(
        byte[] excelContent,
        string sheetName,
        byte[] imageData,
        string cellAddress,
        ImageOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// محافظت از Workbook
    /// </summary>
    Task<byte[]> ProtectWorkbookAsync(
        byte[] excelContent,
        string password,
        WorkbookProtectionOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// تبدیل Excel به CSV
    /// </summary>
    Task<string> ConvertToCsvAsync(
        byte[] excelContent,
        string? sheetName = null,
        char delimiter = ',',
        CancellationToken cancellationToken = default);

    /// <summary>
    /// اضافه کردن Sparkline
    /// </summary>
    Task<byte[]> AddSparklineAsync(
        byte[] excelContent,
        string sheetName,
        string dataRange,
        string locationCell,
        SparklineType type,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// قانون Conditional Formatting
/// </summary>
public class ConditionalFormattingRule
{
    public ConditionalFormattingType Type { get; set; }
    public string? Formula { get; set; }
    public object? Value { get; set; }
    public object? Value2 { get; set; }
    public string BackgroundColor { get; set; } = "#FFEB9C";
    public string ForegroundColor { get; set; } = "#9C5700";
    public bool Bold { get; set; } = false;
}

public enum ConditionalFormattingType
{
    CellValue,
    Expression,
    ColorScale,
    DataBar,
    IconSet,
    TopBottom,
    UniqueValues,
    DuplicateValues
}

/// <summary>
/// قانون اعتبارسنجی داده
/// </summary>
public class DataValidationRule
{
    public DataValidationType Type { get; set; }
    public string? Formula1 { get; set; }
    public string? Formula2 { get; set; }
    public string? ErrorTitle { get; set; }
    public string? ErrorMessage { get; set; }
    public string? PromptTitle { get; set; }
    public string? PromptMessage { get; set; }
    public bool ShowErrorMessage { get; set; } = true;
    public bool ShowInputMessage { get; set; } = true;
}

public enum DataValidationType
{
    None,
    WholeNumber,
    Decimal,
    List,
    Date,
    Time,
    TextLength,
    Custom
}

/// <summary>
/// تنظیمات تصویر
/// </summary>
public class ImageOptions
{
    public int? Width { get; set; }
    public int? Height { get; set; }
    public bool MaintainAspectRatio { get; set; } = true;
    public int OffsetX { get; set; } = 0;
    public int OffsetY { get; set; } = 0;
}

/// <summary>
/// تنظیمات محافظت Workbook
/// </summary>
public class WorkbookProtectionOptions
{
    public bool LockStructure { get; set; } = true;
    public bool LockWindows { get; set; } = false;
}

/// <summary>
/// نوع Sparkline
/// </summary>
public enum SparklineType
{
    Line,
    Column,
    WinLoss
}