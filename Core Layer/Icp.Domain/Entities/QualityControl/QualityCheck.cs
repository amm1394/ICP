using Core.Icp.Domain.Base;
using Core.Icp.Domain.Entities.Samples;
using Core.Icp.Domain.Enums;  // ← این رو داشته باش

namespace Core.Icp.Domain.Entities.QualityControl
{
    public class QualityCheck : BaseEntity
    {
        public CheckType CheckType { get; set; }  // ← باید enum باشه
        public CheckStatus Status { get; set; }  // ← باید enum باشه
        public string? Message { get; set; }
        public string? Details { get; set; }

        // Foreign Keys
        public Guid SampleId { get; set; }

        // Navigation Properties
        public Sample Sample { get; set; } = null!;
    }
}