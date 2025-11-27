using Domain.Reports.DTOs;

namespace Domain.Interfaces;

public interface IExcelExportService
{
    /// <summary>
    /// تبدیل گزارش پیوت به فایل اکسل (byte array)
    /// </summary>
    byte[] ExportToExcel(PivotReportDto reportData);
}