namespace Infrastructure.Icp.Reports.Models
{
    /// <summary>
    /// آمار یک عنصر
    /// </summary>
    public class ElementStatistics
    {
        public string ElementSymbol { get; set; } = string.Empty;
        public int Isotope { get; set; }
        public int TotalMeasurements { get; set; }
        public int ValidMeasurements { get; set; }
        public int InvalidMeasurements { get; set; }

        // Intensity Statistics
        public decimal MinIntensity { get; set; }
        public decimal MaxIntensity { get; set; }
        public decimal AverageIntensity { get; set; }
        public decimal MedianIntensity { get; set; }
        public decimal StdDevIntensity { get; set; }
        public decimal RSDIntensity { get; set; } // Relative Standard Deviation (%)

        // Concentration Statistics
        public decimal? MinConcentration { get; set; }
        public decimal? MaxConcentration { get; set; }
        public decimal? AverageConcentration { get; set; }
        public decimal? MedianConcentration { get; set; }
        public decimal? StdDevConcentration { get; set; }
        public decimal? RSDConcentration { get; set; }

        public string? Unit { get; set; }

        // Detection
        public int BelowDetectionLimit { get; set; }
        public decimal? DetectionLimit { get; set; }
    }
}