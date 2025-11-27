using ClosedXML.Excel;
using Domain.Interfaces;
using Domain.Reports.DTOs;

namespace Infrastructure.Reports;

public class ExcelExportService : IExcelExportService
{
    public byte[] ExportToExcel(PivotReportDto reportData)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Final Results");

        // --- 1. ایجاد هدرها ---
        int colIndex = 1;

        // ستون‌های ثابت
        worksheet.Cell(1, colIndex++).Value = "Solution Label";
        worksheet.Cell(1, colIndex++).Value = "Sample Type";
        worksheet.Cell(1, colIndex++).Value = "Replicates";

        // ستون‌های عنصری (دینامیک)
        foreach (var element in reportData.Columns)
        {
            // ادغام سلول‌ها برای Value و RSD
            var headerCell = worksheet.Cell(1, colIndex);
            headerCell.Value = element;
            headerCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // ستون‌های زیرمجموعه (Mean, RSD)
            worksheet.Cell(2, colIndex).Value = "Conc (ppm)";
            worksheet.Cell(2, colIndex + 1).Value = "RSD %";

            // Merge کردن هدر اصلی (نام عنصر) روی دو ستون
            worksheet.Range(1, colIndex, 1, colIndex + 1).Merge();

            colIndex += 2;
        }

        // استایل هدر
        var headerRange = worksheet.Range(1, 1, 2, colIndex - 1);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
        headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

        // --- 2. پر کردن داده‌ها ---
        int rowIndex = 3;
        foreach (var row in reportData.Rows)
        {
            int currentCol = 1;

            // اطلاعات ثابت
            worksheet.Cell(rowIndex, currentCol++).Value = row.SolutionLabel;
            worksheet.Cell(rowIndex, currentCol++).Value = row.SampleType;
            worksheet.Cell(rowIndex, currentCol++).Value = row.ReplicateCount;

            // مقادیر عناصر
            foreach (var element in reportData.Columns)
            {
                if (row.ElementValues.TryGetValue(element, out var result))
                {
                    // مقدار میانگین
                    var valCell = worksheet.Cell(rowIndex, currentCol);
                    valCell.Value = result.Value;
                    valCell.Style.NumberFormat.Format = "0.000"; // فرمت سه رقم اعشار

                    // مقدار RSD
                    var rsdCell = worksheet.Cell(rowIndex, currentCol + 1);
                    rsdCell.Value = result.RSD;
                    rsdCell.Style.NumberFormat.Format = "0.00";

                    // رنگ‌آمیزی RSDهای بالا (هشدار)
                    if (result.RSD > 5.0) // مثلاً اگر RSD بیشتر از 5% بود قرمز شود
                    {
                        rsdCell.Style.Font.FontColor = XLColor.Red;
                    }
                }
                else
                {
                    worksheet.Cell(rowIndex, currentCol).Value = "-";
                    worksheet.Cell(rowIndex, currentCol + 1).Value = "-";
                }
                currentCol += 2;
            }
            rowIndex++;
        }

        // تنظیم خودکار عرض ستون‌ها
        worksheet.Columns().AdjustToContents();

        // خروجی به صورت بایت
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}