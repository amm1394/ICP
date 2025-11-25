using Domain.Common;

namespace Domain.Entities;

public class CalibrationPoint : BaseEntity
{
    public Guid CalibrationCurveId { get; set; }
    public virtual CalibrationCurve CalibrationCurve { get; set; } = null!;

    public double Concentration { get; set; } // محور X (استاندارد)
    public double Intensity { get; set; }     // محور Y (دستگاه)

    public bool IsExcluded { get; set; } = false; // اگر کاربر دستی نقطه‌ای را حذف کند
}