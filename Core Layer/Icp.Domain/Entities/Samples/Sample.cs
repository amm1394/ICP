using Core.Icp.Domain.Base;
using Core.Icp.Domain.Entities.Projects;
using Core.Icp.Domain.Entities.QualityControl;
using Core.Icp.Domain.Enums;

namespace Core.Icp.Domain.Entities.Samples
{
    /// <summary>
    /// نمایانگر یک نمونه آزمایشگاهی
    /// </summary>
    public class Sample : BaseEntity
    {
        public string SampleId { get; set; } = string.Empty;
        public string SampleName { get; set; } = string.Empty;
        public DateTime RunDate { get; set; }
        public SampleStatus Status { get; set; }

        // پارامترهای فیزیکی
        public decimal Weight { get; set; }
        public decimal Volume { get; set; }
        public decimal DilutionFactor { get; set; }

        public string? Notes { get; set; }

        // Foreign Keys
        public Guid ProjectId { get; set; }

        // Navigation Properties
        public Project Project { get; set; } = null!;
        public ICollection<Measurement> Measurements { get; set; } = new List<Measurement>();
        public ICollection<QualityCheck> QualityChecks { get; set; } = new List<QualityCheck>();
    }
}