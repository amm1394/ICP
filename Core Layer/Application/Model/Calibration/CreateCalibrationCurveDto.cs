namespace Core.Icp.Application.Models.Calibration
{
    /// <summary>
    /// ورودی برای ساختن منحنی کالیبراسیون از روی نقاط آماده
    /// (در قدم بعد می‌تونیم ورودی رو از روی داده‌های پروژه بسازیم)
    /// </summary>
    public class CreateCalibrationCurveDto
    {
        public Guid ElementId { get; set; }

        public Guid ProjectId { get; set; }

        public string FitType { get; set; } = "Linear";

        public int Degree { get; set; } = 1;

        public string? SettingsJson { get; set; }

        public IList<CreateCalibrationPointDto> Points { get; set; }
            = new List<CreateCalibrationPointDto>();
    }
}
