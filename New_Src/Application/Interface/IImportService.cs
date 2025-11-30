using Application.DTOs;
using Shared.Wrapper;

namespace Application.Services;

public interface IImportService
{
    /// <summary>
    /// Import CSV stream into a new project (basic import). 
    /// </summary>
    Task<Result<ProjectSaveResult>> ImportCsvAsync(
        Stream csvStream,
        string projectName,
        string? owner = null,
        string? stateJson = null,
        IProgress<(int total, int processed)>? progress = null);

    /// <summary>
    /// Advanced import with format detection and options
    /// </summary>
    Task<Result<AdvancedImportResult>> ImportAdvancedAsync(
        Stream fileStream,
        string fileName,
        AdvancedImportRequest request,
        IProgress<(int total, int processed, string message)>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Detect file format without importing
    /// </summary>
    Task<Result<FileFormatDetectionResult>> DetectFormatAsync(
        Stream fileStream,
        string fileName);

    /// <summary>
    /// Preview file content before import
    /// </summary>
    Task<Result<FilePreviewResult>> PreviewFileAsync(
        Stream fileStream,
        string fileName,
        int previewRows = 10);

    /// <summary>
    /// Import additional file and append to existing project
    /// </summary>
    Task<Result<AdvancedImportResult>> ImportAdditionalAsync(
        Guid projectId,
        Stream fileStream,
        string fileName,
        AdvancedImportRequest? request = null,
        IProgress<(int total, int processed, string message)>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Import Excel file (xlsx/xls)
    /// </summary>
    Task<Result<AdvancedImportResult>> ImportExcelAsync(
        Stream fileStream,
        string fileName,
        AdvancedImportRequest request,
        IProgress<(int total, int processed, string message)>? progress = null,
        CancellationToken cancellationToken = default);
}