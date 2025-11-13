// Infrastructure.Icp.Reports/Interfaces/IReportGenerator.cs
namespace Infrastructure.Icp.Reports.Interfaces;

public interface IReportGenerator
{
    /// <summary>
    /// تولید گزارش و ذخیره در مسیر مشخص شده
    /// </summary>
    Task<string> GenerateReportAsync<TData>(TData data, string templateName, string outputPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// تولید گزارش و برگرداندن به صورت byte array
    /// </summary>
    Task<byte[]> GenerateReportAsBytesAsync<TData>(TData data, string templateName, CancellationToken cancellationToken = default);

    /// <summary>
    /// تولید گزارش و برگرداندن به صورت Stream
    /// </summary>
    Task<Stream> GenerateReportAsStreamAsync<TData>(TData data, string templateName, CancellationToken cancellationToken = default);

    /// <summary>
    /// فرمت خروجی گزارش (PDF, Excel, etc.)
    /// </summary>
    string OutputFormat { get; }
}