using Core.Icp.Domain.Base;
using Core.Icp.Domain.Entities.Elements;

public class CRMValue : BaseEntity
{
    public Guid CRMId { get; set; }
    public virtual CRM CRM { get; set; } = default!;

    public Guid ElementId { get; set; }
    public virtual Element Element { get; set; } = default!;

    public decimal CertifiedValue { get; set; }
    public decimal? Uncertainty { get; set; }
    public string Unit { get; set; } = "ppm";

    public decimal? MinAcceptable { get; set; }
    public decimal? MaxAcceptable { get; set; }

    public bool IsActive { get; set; } = true;
}
