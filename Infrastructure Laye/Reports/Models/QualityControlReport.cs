namespace Infrastructure.Icp.Reports.Models
{
    /// <summary>
    /// گزارش کنترل کیفیت
    /// </summary>
    public class QualityControlReport
    {
        public int TotalSamples { get; set; }
        public int PassedSamples { get; set; }
        public int FailedSamples { get; set; }
        public decimal PassRate { get; set; }

        public WeightCheckReport WeightCheck { get; set; } = new();
        public VolumeCheckReport VolumeCheck { get; set; } = new();
        public DFCheckReport DFCheck { get; set; } = new();
        public EmptyCheckReport EmptyCheck { get; set; } = new();

        public List<string> FailedSampleIds { get; set; } = new();
        public Dictionary<string, List<string>> FailureReasons { get; set; } = new();
    }

    public class WeightCheckReport
    {
        public int TotalChecked { get; set; }
        public int Passed { get; set; }
        public int Failed { get; set; }
        public decimal MinWeight { get; set; }
        public decimal MaxWeight { get; set; }
        public decimal AverageWeight { get; set; }
        public List<string> FailedSamples { get; set; } = new();
    }

    public class VolumeCheckReport
    {
        public int TotalChecked { get; set; }
        public int Passed { get; set; }
        public int Failed { get; set; }
        public decimal MinVolume { get; set; }
        public decimal MaxVolume { get; set; }
        public decimal AverageVolume { get; set; }
        public List<string> FailedSamples { get; set; } = new();
    }

    public class DFCheckReport
    {
        public int TotalChecked { get; set; }
        public int Passed { get; set; }
        public int Failed { get; set; }
        public int MinDF { get; set; }
        public int MaxDF { get; set; }
        public double AverageDF { get; set; }
        public List<string> FailedSamples { get; set; } = new();
    }

    public class EmptyCheckReport
    {
        public int TotalChecked { get; set; }
        public int EmptyRows { get; set; }
        public int ValidRows { get; set; }
    }
}