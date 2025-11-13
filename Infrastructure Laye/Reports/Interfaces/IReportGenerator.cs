using Shared.Icp.DTOs.Reports;
using System.Runtime.Serialization;

namespace Infrastructure.Icp.Reports.Interfaces;

/// <summary>
/// رابط پایه برای تولید گزارش
/// </summary>
public interface IReportGenerator
{
    /// <summary>
    /// تولید گزارش و ذخیره در مسیر مشخص شده
    /// </summary>
    Task<ReportResultDto> GenerateReportAsync<TData>(
        TData data,
        string templateName,
        string outputPath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// تولید گزارش و برگرداندن به صورت byte array
    /// </summary>
    Task<byte[]> GenerateReportAsBytesAsync<TData>(
        TData data,
        string templateName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// تولید گزارش و برگرداندن به صورت Stream
    /// </summary>
    Task<Stream> GenerateReportAsStreamAsync<TData>(
        TData data,
        string templateName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// تولید گزارش با تنظیمات سفارشی
    /// </summary>
    Task<ReportResultDto> GenerateReportWithOptionsAsync<TData>(
        TData data,
        string templateName,
        ReportOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// فرمت خروجی گزارش (PDF, Excel, etc.)
    /// </summary>
    string OutputFormat { get; }

    /// <summary>
    /// بررسی معتبر بودن قالب
    /// </summary>
    Task<bool> IsTemplateValidAsync(string templateName);

    /// <summary>
    /// دریافت اطلاعات قالب
    /// </summary>
    Task<TemplateInfoDto> GetTemplateInfoAsync(string templateName);
}