namespace Core.Icp.Application.Models.CRM
{
    public class CreateCRMDto
    {
        public string CRMId { get; set; } = default!;

        public string? Name { get; set; }

        public string? Description { get; set; }

        public string? Matrix { get; set; }

        public string? Manufacturer { get; set; }

        public string? LotNumber { get; set; }

        public DateTime? ExpirationDate { get; set; }

        public IList<CreateCRMValueDto> CertifiedValues { get; set; }
            = new List<CreateCRMValueDto>();
    }
}
