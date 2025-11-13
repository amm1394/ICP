// Infrastructure.Icp.Reports/Interfaces/IPdfReportGenerator.cs
namespace Infrastructure.Icp.Reports.Interfaces;

public interface IPdfReportGenerator : IReportGenerator
{
    /// <summary>
    /// تنظیمات صفحه PDF
    /// </summary>
    Task<byte[]> GeneratePdfWithSettingsAsync<TData>(
        TData data,
        string templateName,
        PdfPageSettings pageSettings,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// تولید گزارش چند صفحه‌ای
    /// </summary>
    Task<byte[]> GenerateMultiPagePdfAsync<TData>(
        IEnumerable<TData> dataPages,
        string templateName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// افزودن واترمارک به PDF
    /// </summary>
    Task<byte[]> AddWatermarkAsync(
        byte[] pdfContent,
        string watermarkText,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// ترکیب چندین PDF
    /// </summary>
    Task<byte[]> MergePdfsAsync(
        IEnumerable<byte[]> pdfFiles,
        CancellationToken cancellationToken = default);
}