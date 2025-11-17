namespace Core.Icp.Application.Models.Calibration
{
    public class CreateCalibrationPointDto
    {
        public decimal Concentration { get; set; }

        public decimal Intensity { get; set; }

        public bool IsUsedInFit { get; set; } = true;

        public int Order { get; set; }

        public string? Label { get; set; }

        public string PointType { get; set; } = "CRM";
    }
}
