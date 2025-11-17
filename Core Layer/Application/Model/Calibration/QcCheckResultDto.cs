namespace Core.Icp.Application.Models.Calibration
{
    public class QcCheckResultDto
    {
        public Guid ProjectId { get; set; }

        public Guid ElementId { get; set; }

        /// <summary>
        /// غلظت واقعی CRM/RM
        /// </summary>
        public decimal ExpectedConcentration { get; set; }

        /// <summary>
        /// غلظت محاسبه‌شده از روی منحنی
        /// </summary>
        public decimal MeasuredConcentration { get; set; }

        /// <summary>
        /// شدت اندازه‌گیری‌شده (Intensity)
        /// </summary>
        public decimal Intensity { get; set; }

        /// <summary>
        /// خطا به درصد
        /// </summary>
        public decimal ErrorPercent { get; set; }

        /// <summary>
        /// تلورانس مجاز به درصد (مثلاً ±5%)
        /// </summary>
        public decimal TolerancePercent { get; set; }

        /// <summary>
        /// پاس/فیل QC
        /// </summary>
        public bool IsPass { get; set; }

        public string? Message { get; set; }
    }
}
