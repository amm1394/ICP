using System;
using System.Collections.Generic;

namespace Core.Icp.Application.Models.Calibration
{
    public class CalibrationCurveDto
    {
        public Guid Id { get; set; }

        public Guid ElementId { get; set; }

        public Guid ProjectId { get; set; }

        public decimal Slope { get; set; }

        public decimal Intercept { get; set; }

        public decimal RSquared { get; set; }

        public string FitType { get; set; } = "Linear";

        public int Degree { get; set; } = 1;

        public bool IsActive { get; set; }

        public string? SettingsJson { get; set; }

        public IReadOnlyCollection<CalibrationPointDto> Points { get; set; }
            = Array.Empty<CalibrationPointDto>();
    }
}
