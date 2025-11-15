using Core.Icp.Domain.Enums;
using Infrastructure.Icp.Reports.Models;
using Infrastructure.Icp.Reports.Models.Configurations;
using Shared.Icp.DTOs.Reports;

namespace Infrastructure.Icp.Reports.Interfaces;

/// <summary>
/// رابط تولید گزارش PDF
/// </summary>
public interface IPdfReportGenerator : IReportGenerator
{
    /// <summary>
    /// تولید PDF با تنظیمات صفحه
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
        WatermarkOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// ترکیب چندین PDF
    /// </summary>
    Task<byte[]> MergePdfsAsync(
        IEnumerable<byte[]> pdfFiles,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// اضافه کردن هدر و فوتر
    /// </summary>
    Task<byte[]> AddHeaderFooterAsync(
        byte[] pdfContent,
        string? headerText,
        string? footerText,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// تبدیل HTML به PDF
    /// </summary>
    Task<byte[]> ConvertHtmlToPdfAsync(
        string htmlContent,
        PdfPageSettings? settings = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// افزودن شماره صفحه
    /// </summary>
    Task<byte[]> AddPageNumbersAsync(
        byte[] pdfContent,
        PageNumberPosition position,
        string? format = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// رمزگذاری PDF
    /// </summary>
    Task<byte[]> EncryptPdfAsync(
        byte[] pdfContent,
        string userPassword,
        string? ownerPassword = null,
        PdfPermissions permissions = PdfPermissions.All,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// استخراج متن از PDF
    /// </summary>
    Task<string> ExtractTextFromPdfAsync(
        byte[] pdfContent,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// افزودن Bookmark به PDF
    /// </summary>
    Task<byte[]> AddBookmarksAsync(
        byte[] pdfContent,
        List<PdfBookmark> bookmarks,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// تنظیمات واترمارک
/// </summary>
public class WatermarkOptions
{
    public string Text { get; set; } = string.Empty;
    public string FontFamily { get; set; } = "Arial";
    public int FontSize { get; set; } = 48;
    public string Color { get; set; } = "#CCCCCC";
    public int Opacity { get; set; } = 50;
    public double RotationAngle { get; set; } = -45;
    public WatermarkPosition Position { get; set; } = WatermarkPosition.Center;
}

public enum WatermarkPosition
{
    Center,
    TopLeft,
    TopCenter,
    TopRight,
    BottomLeft,
    BottomCenter,
    BottomRight
}

/// <summary>
/// مجوزهای PDF
/// </summary>
[Flags]
public enum PdfPermissions
{
    None = 0,
    Print = 1,
    ModifyContents = 2,
    Copy = 4,
    ModifyAnnotations = 8,
    FillIn = 16,
    ScreenReaders = 32,
    Assembly = 64,
    PrintHighQuality = 128,
    All = Print | ModifyContents | Copy | ModifyAnnotations | FillIn | ScreenReaders | Assembly | PrintHighQuality
}

/// <summary>
/// Bookmark در PDF
/// </summary>
public class PdfBookmark
{
    public string Title { get; set; } = string.Empty;
    public int PageNumber { get; set; }
    public List<PdfBookmark> Children { get; set; } = new();
}