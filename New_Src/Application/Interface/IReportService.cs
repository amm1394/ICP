using Application.DTOs;
using Shared.Wrapper;

namespace Application.Services;

/// <summary>
/// Service for generating reports and exports. 
/// </summary>
public interface IReportService
{
    /// <summary>
    /// Generate a report based on request
    /// </summary>
    Task<Result<ReportResultDto>> GenerateReportAsync(ReportRequest request);

    /// <summary>
    /// Export data to specified format
    /// </summary>
    Task<Result<byte[]>> ExportDataAsync(ExportRequest request);

    /// <summary>
    /// Export to Excel with multiple sheets
    /// </summary>
    Task<Result<byte[]>> ExportToExcelAsync(Guid projectId, ReportOptions? options = null);

    /// <summary>
    /// Export to CSV
    /// </summary>
    Task<Result<byte[]>> ExportToCsvAsync(Guid projectId, bool useOxide = false);

    /// <summary>
    /// Export to JSON
    /// </summary>
    Task<Result<byte[]>> ExportToJsonAsync(Guid projectId);

    /// <summary>
    /// Generate HTML report
    /// </summary>
    Task<Result<string>> GenerateHtmlReportAsync(Guid projectId, ReportOptions? options = null);

    /// <summary>
    /// Calculate calibration ranges for each element/wavelength
    /// Based on Python report.py logic: min/max from Blk data Soln Conc
    /// </summary>
    Task<Result<Dictionary<string, CalibrationRange>>> GetCalibrationRangesAsync(Guid projectId);

    /// <summary>
    /// Select best wavelength for each base element per row
    /// Based on Python report.py select_best_wavelength_for_row()
    /// </summary>
    Task<Result<BestWavelengthResult>> SelectBestWavelengthsAsync(BestWavelengthRequest request);
}