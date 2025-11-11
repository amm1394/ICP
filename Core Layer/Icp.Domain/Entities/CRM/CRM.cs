using Core.Icp.Domain.Base;

namespace Core.Icp.Domain.Entities.CRM
{
    /// <summary>
    /// ماده مرجع گواهی شده
    /// </summary>
    public class CRM : BaseEntity
    {
        public string CRMId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        public string? Manufacturer { get; set; }
        public string? LotNumber { get; set; }
        public DateTime? ExpirationDate { get; set; }

        public bool IsActive { get; set; } = true;
        public string? Notes { get; set; }

        // Navigation Properties
        public ICollection<CRMValue> CertifiedValues { get; set; } = new List<CRMValue>();
    }
}