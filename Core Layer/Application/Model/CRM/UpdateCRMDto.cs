namespace Core.Icp.Application.Models.CRM
{
    public class UpdateCRMDto
    {
        public Guid Id { get; set; }

        public string CRMId { get; set; } = default!;

        public string? Name { get; set; }

        public string? Description { get; set; }

        public string? Matrix { get; set; }

        public string? Manufacturer { get; set; }

        public string? LotNumber { get; set; }

        public DateTime? ExpirationDate { get; set; }

        public bool IsActive { get; set; }

        public IList<UpdateCRMValueDto> CertifiedValues { get; set; } = new List<UpdateCRMValueDto>();
    }
}