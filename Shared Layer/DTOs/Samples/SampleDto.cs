using Shared.Icp.DTOs.Common;

namespace Shared.Icp.DTOs.Samples
{
    public class SampleDto : BaseDto  // ✅ باید از BaseDto ارث ببره
    {
        public string SampleId { get; set; } = string.Empty;
        public string SampleName { get; set; } = string.Empty;
        public DateTime RunDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal Weight { get; set; }
        public decimal Volume { get; set; }
        public decimal DilutionFactor { get; set; }
        public string? Notes { get; set; }
        public Guid ProjectId { get; set; }
    }
}