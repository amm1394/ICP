using System;
using System.Collections.Generic;
using Core.Icp.Domain.Entities.Elements;
using Core.Icp.Domain.Entities.Projects;

namespace Core.Icp.Domain.Entities.Calibration
{
    public class CalibrationCurve // : BaseEntity
    {
        public Guid Id { get; set; }

        /// <summary>
        /// عنصر مربوطه (مثلاً Cu)
        /// </summary>
        public Guid ElementId { get; set; }
        public virtual Element Element { get; set; } = default!;

        /// <summary>
        /// پروژه‌ای که این منحنی برای آن ساخته شده
        /// </summary>
        public Guid ProjectId { get; set; }
        public virtual Project Project { get; set; } = default!;

        /// <summary>
        /// شیب منحنی (Slope)
        /// </summary>
        public decimal Slope { get; set; }

        /// <summary>
        /// عرض از مبدأ (Intercept)
        /// </summary>
        public decimal Intercept { get; set; }

        /// <summary>
        /// ضریب تعیین (R²)
        /// </summary>
        public decimal RSquared { get; set; }

        /// <summary>
        /// نوع برازش (Linear, WeightedLinear, Polynomial, ...)
        /// </summary>
        public string FitType { get; set; } = "Linear";

        /// <summary>
        /// درجه چندجمله‌ای (برای Polynomial)، در Linear = 1
        /// </summary>
        public int Degree { get; set; } = 1;

        /// <summary>
        /// توضیحات یا تنظیمات اضافی (مثلاً نوع وزن‌دهی)
        /// </summary>
        public string? SettingsJson { get; set; }

        /// <summary>
        /// آیا این منحنی در حال حاضر منحنی فعال برای این پروژه/عنصر است؟
        /// </summary>
        public bool IsActive { get; set; } = true;

        public virtual ICollection<CalibrationPoint> Points { get; set; } = new List<CalibrationPoint>();
    }
}
