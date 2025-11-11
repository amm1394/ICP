using Core.Icp.Domain.Base;
using Core.Icp.Domain.Entities.Elements;

namespace Core.Icp.Domain.Entities.CRM
{
    /// <summary>
    /// مقدار گواهی شده یک عنصر در CRM
    /// </summary>
    public class CRMValue : BaseEntity
    {
        public Guid CRMId { get; set; }
        public Guid ElementId { get; set; }

        public decimal CertifiedValue { get; set; }
        public decimal? Uncertainty { get; set; }
        public string Unit { get; set; } = "ppm";

        // Navigation Properties
        public CRM CRM { get; set; } = null!;
        public Element Element { get; set; } = null!;
    }
}