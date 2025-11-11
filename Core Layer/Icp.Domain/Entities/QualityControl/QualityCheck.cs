using Core.Icp.Domain.Base;
using Core.Icp.Domain.Entities.Samples;
using Core.Icp.Domain.Enums;

namespace Core.Icp.Domain.Entities.QualityControl
{
    /// <summary>
    /// کنترل کیفیت برای یک نمونه
    /// </summary>
    public class QualityCheck : BaseEntity
    {
        public Guid SampleId { get; set; }

        public CheckType CheckType { get; set; }
        public CheckStatus Status { get; set; }
        public DateTime CheckDate { get; set; }

        public string? Message { get; set; }

        // Navigation Properties
        public Sample Sample { get; set; } = null!;
    }
}