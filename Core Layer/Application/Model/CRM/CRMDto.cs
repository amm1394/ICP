using System;
using System.Collections.Generic;

namespace Core.Icp.Application.Models.CRM
{
    public class CRMDto
    {
        public Guid Id { get; set; }

        /// <summary>
        /// کد CRM مثل "OREAS 233"
        /// </summary>
        public string CRMId { get; set; } = default!;

        public string? Name { get; set; }

        public string? Description { get; set; }

        public string? Matrix { get; set; }

        public string? Manufacturer { get; set; }

        public string? LotNumber { get; set; }

        public DateTime? ExpirationDate { get; set; }

        public bool IsActive { get; set; }

        public IReadOnlyCollection<CRMValueDto> CertifiedValues { get; set; } = Array.Empty<CRMValueDto>();
    }
}
