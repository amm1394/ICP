using Domain.Reports.DTOs;

namespace Domain.Interfaces; // اصلاح شده: حذف Core.Icp

public interface IReportGenerator
{
    byte[] GenerateExcel(PivotReportDto data);
}