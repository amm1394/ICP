namespace Infrastructure.Icp.Reports.Models
{
    public class WeightCheckReport
    {
        public int TotalChecked { get; set; }
        public int Passed { get; set; }
        public int Failed { get; set; }
        public decimal MinWeight { get; set; }
        public decimal MaxWeight { get; set. }
        public decimal AverageWeight { get; set; }
        public List<string> FailedSamples { get; set; } = new();
    }
}
