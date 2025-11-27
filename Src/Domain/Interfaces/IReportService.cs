using Domain.Reports.DTOs;

namespace Domain.Interfaces; // ✅ فضای نام استاندارد

public interface IReportService
{
    /// <summary>
    /// دریافت گزارش به صورت Pivot (برای نمایش در UI یا ارسال به اکسل)
    /// </summary>
    Task<PivotReportDto> GetPivotReportAsync(
        Guid projectId,
        bool useConcentration = true,
        CancellationToken cancellationToken = default);
}