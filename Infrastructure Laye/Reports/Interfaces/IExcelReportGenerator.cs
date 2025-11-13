// Infrastructure.Icp.Reports/Interfaces/IExcelReportGenerator.cs
namespace Infrastructure.Icp.Reports.Interfaces;

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
}