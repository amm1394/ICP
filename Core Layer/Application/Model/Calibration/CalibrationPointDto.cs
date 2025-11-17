using System;

namespace Core.Icp.Application.Models.Calibration
{
    public class CalibrationPointDto
    {
        public Guid Id { get; set; }

        public decimal Concentration { get; set; }

        public decimal Intensity { get; set; }

        public bool IsUsedInFit { get; set; }

        public int Order { get; set; }

        public string? Label { get; set; }

        public string PointType { get; set; } = "CRM";
    }
}
