namespace Core.Icp.Application.Models.Calibration
{
    /// <summary>
    /// ورودی کلی برای اعمال کالیبراسیون روی چند نمونه
    /// </summary>
    public class ApplyCalibrationRequestDto
    {
        public Guid ProjectId { get; set; }
        public Guid ElementId { get; set; }

        /// <summary>
        /// لیست نمونه‌ها (Intensityها) که باید غلظت‌شان محاسبه شود
        /// </summary>
        public IList<CalibratedSampleRequestDto> Samples { get; set; }
            = new List<CalibratedSampleRequestDto>();
    }

    /// <summary>
    /// ورودی هر نمونه (مثلاً یک SampleCode از پروژه)
    /// </summary>
    public class CalibratedSampleRequestDto
    {
        /// <summary>
        /// شناسه/کد نمونه (می‌تواند SampleId، SampleCode، RowIndex و ... باشد)
        /// </summary>
        public string SampleKey { get; set; } = default!;

        /// <summary>
        /// شدت اندازه‌گیری‌شده
        /// </summary>
        public decimal Intensity { get; set; }
    }

    /// <summary>
    /// خروجی محاسبه برای هر نمونه
    /// </summary>
    public class CalibratedSampleResultDto
    {
        public string SampleKey { get; set; } = default!;
        public decimal Intensity { get; set; }
        public decimal Concentration { get; set; }

        /// <summary>
        /// اگر منحنی یا محاسبه‌ای مسئله داشته، پیام خطا
        /// </summary>
        public string? Error { get; set; }
    }
}
