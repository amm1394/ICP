using Domain.Interfaces;
using Domain.Reports.DTOs;             // برای PivotReportDto
using Microsoft.AspNetCore.Mvc;
using Shared.Wrapper;

namespace Isatis.Api.Controllers;

[Route("api/projects/{projectId}/reports")]
[ApiController]
public class ReportController(
    IReportService reportService,
    IExcelExportService excelExportService // 👈 تزریق سرویس اکسل
    ) : ControllerBase
{
    /// <summary>
    /// دریافت گزارش ماتریسی (Pivot) برای نمایش در جدول
    /// </summary>
    [HttpGet("pivot")]
    public async Task<ActionResult<Result<PivotReportDto>>> GetPivotReport(Guid projectId)
    {
        // ✅ نام متد اصلاح شد (قبلاً GetProjectPivotReportAsync بود)
        var report = await reportService.GetPivotReportAsync(projectId);

        return Ok(await Result<PivotReportDto>.SuccessAsync(report));
    }

    /// <summary>
    /// دانلود فایل اکسل گزارش نهایی
    /// </summary>
    [HttpGet("export/excel")]
    public async Task<IActionResult> ExportExcel(Guid projectId)
    {
        // 1. ابتدا داده‌ها را از سرویس گزارش می‌گیریم
        var reportData = await reportService.GetPivotReportAsync(projectId);

        if (reportData.Rows.Count == 0)
            return BadRequest("No data found to export.");

        // 2. سپس داده‌ها را به سرویس اکسل می‌دهیم تا فایل بسازد
        // ✅ این متد جایگزین ExportProjectToExcelAsync شد
        var fileContent = excelExportService.ExportToExcel(reportData);

        // 3. ارسال فایل برای دانلود
        string fileName = $"Project_{projectId}_Results_{DateTime.Now:yyyyMMdd}.xlsx";
        return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }
}