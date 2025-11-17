using Core.Icp.Domain.Base;

public class CRM : BaseEntity
{
    public string CRMId { get; set; } = default!;
    public string? Name { get; set; }
    public string? Manufacturer { get; set; }
    public string? LotNumber { get; set; }
    public string? Matrix { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Description { get; set; }
    public DateTime? ExpirationDate { get; set; }

    public virtual ICollection<CRMValue> CertifiedValues { get; set; } = new List<CRMValue>();
}