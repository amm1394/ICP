namespace Core.Icp.Application.Models.CRM
{
    public class UpdateCRMValueDto
    {
        public Guid? Id { get; set; }  // null = مقدار جدید

        public Guid ElementId { get; set; }

        public string? Unit { get; set; }

        public decimal CertifiedValue { get; set; }

        public decimal? Uncertainty { get; set; }

        public decimal? MinAcceptable { get; set; }

        public decimal? MaxAcceptable { get; set; }

        public bool IsActive { get; set; } = true;
    }
}