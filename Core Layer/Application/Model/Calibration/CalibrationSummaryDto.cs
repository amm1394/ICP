namespace Core.Icp.Application.Models.Calibration
{
    public class CalibrationSummaryDto
    {
        public Guid ProjectId { get; set; }

        public Guid ElementId { get; set; }

        public Guid CurveId { get; set; }

        public decimal Slope { get; set; }

        public decimal Intercept { get; set; }

        public decimal RSquared { get; set; }

        public int PointsCount { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool IsActive { get; set; }

        public string FitType { get; set; } = "Linear";

        public int Degree { get; set; } = 1;
    }
}
