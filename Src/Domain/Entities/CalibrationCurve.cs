using Domain.Common;

namespace Domain.Entities;

public class CalibrationCurve : BaseEntity
{
    public Guid ProjectId { get; set; }
    public string ElementName { get; set; } = string.Empty; // مثلا "Cu" یا "Fe 56"

    // ضرایب خط: y = mx + b
    // m = Slope (شیب)
    // b = Intercept (عرض از مبدأ)
    public double Slope { get; set; }
    public double Intercept { get; set; }
    public double RSquared { get; set; } // ضریب تعیین (دقت منحنی)

    public bool IsActive { get; set; } = true;

    // نقاطی که این منحنی را ساخته‌اند
    public virtual ICollection<CalibrationPoint> Points { get; set; } = new List<CalibrationPoint>();
}