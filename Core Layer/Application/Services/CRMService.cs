using Core.Icp.Application.Interfaces;
using Core.Icp.Application.Models.CRM;
using Core.Icp.Domain.Interfaces.Repositories;

namespace Core.Icp.Application.Services
{
    public class CRMService : ICRMService
    {
        private readonly ICRMRepository _crmRepository;

        public CRMService(ICRMRepository crmRepository)
        {
            _crmRepository = crmRepository;
        }

        public async Task<IReadOnlyCollection<CRMDto>> GetCrmsAsync(
            bool onlyActive = true,
            CancellationToken cancellationToken = default)
        {
            var crms = await _crmRepository.GetAllAsync(cancellationToken);

            if (onlyActive)
            {
                crms = crms
                    .Where(c => c.IsActive &&
                                (!c.ExpirationDate.HasValue || c.ExpirationDate >= DateTime.Today))
                    .ToList();
            }

            return crms
                .Select(MapToDto)
                .ToList();
        }

        public async Task<CRMDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var crm = await _crmRepository.GetByIdAsync(id, cancellationToken);
            if (crm == null)
                return null;

            return MapToDto(crm);
        }

        public async Task<IReadOnlyCollection<CRMDto>> SearchAsync(
            string? crmId = null,
            string? matrix = null,
            bool onlyActive = true,
            CancellationToken cancellationToken = default)
        {
            var crms = await _crmRepository.FindAsync(c =>
                    (string.IsNullOrEmpty(crmId) || c.CRMId.Contains(crmId)) &&
                    (string.IsNullOrEmpty(matrix) || c.Matrix == matrix),
                cancellationToken);

            if (onlyActive)
            {
                crms = crms
                    .Where(c => c.IsActive &&
                                (!c.ExpirationDate.HasValue || c.ExpirationDate >= DateTime.Today))
                    .ToList();
            }

            return crms
                .Select(MapToDto)
                .ToList();
        }

        public async Task<CRMDto> CreateAsync(CreateCRMDto dto, CancellationToken cancellationToken = default)
        {
            var crm = new CRM
            {
                Id = Guid.NewGuid(),
                CRMId = dto.CRMId,
                Name = dto.Name,
                Description = dto.Description,
                Matrix = dto.Matrix,
                Manufacturer = dto.Manufacturer,
                LotNumber = dto.LotNumber,
                ExpirationDate = dto.ExpirationDate,
                IsActive = true,
                CertifiedValues = dto.CertifiedValues.Select(v => new CRMValue
                {
                    Id = Guid.NewGuid(),
                    ElementId = v.ElementId,
                    Unit = v.Unit ?? "ppm",
                    CertifiedValue = v.CertifiedValue,
                    Uncertainty = v.Uncertainty,
                    MinAcceptable = v.MinAcceptable,
                    MaxAcceptable = v.MaxAcceptable,
                    IsActive = v.IsActive
                }).ToList()
            };

            crm = await _crmRepository.AddAsync(crm, cancellationToken);

            return MapToDto(crm);
        }

        public async Task<CRMDto> UpdateAsync(UpdateCRMDto dto, CancellationToken cancellationToken = default)
        {
            var crm = await _crmRepository.GetByIdAsync(dto.Id, cancellationToken);
            if (crm == null)
                throw new InvalidOperationException($"CRM with id {dto.Id} not found.");

            crm.CRMId = dto.CRMId;
            crm.Name = dto.Name;
            crm.Description = dto.Description;
            crm.Matrix = dto.Matrix;
            crm.Manufacturer = dto.Manufacturer;
            crm.LotNumber = dto.LotNumber;
            crm.ExpirationDate = dto.ExpirationDate;
            crm.IsActive = dto.IsActive;

            UpdateCertifiedValues(crm, dto);

            crm = await _crmRepository.UpdateAsync(crm, cancellationToken);

            return MapToDto(crm);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            await _crmRepository.DeleteAsync(id, cancellationToken);
        }

        #region Helpers

        private static CRMDto MapToDto(CRM crm)
        {
            return new CRMDto
            {
                Id = crm.Id,
                CRMId = crm.CRMId,
                Name = crm.Name,
                Description = crm.Description,
                Matrix = crm.Matrix,
                Manufacturer = crm.Manufacturer,
                LotNumber = crm.LotNumber,
                ExpirationDate = crm.ExpirationDate,
                IsActive = crm.IsActive,
                CertifiedValues = crm.CertifiedValues?
                    .Select(v => new CRMValueDto
                    {
                        Id = v.Id,
                        ElementId = v.ElementId,
                        Unit = v.Unit,
                        CertifiedValue = v.CertifiedValue,
                        Uncertainty = v.Uncertainty,
                        MinAcceptable = v.MinAcceptable,
                        MaxAcceptable = v.MaxAcceptable,
                        IsActive = v.IsActive
                    })
                    .ToList()
                    ?? new List<CRMValueDto>()
            };
        }

        private static void UpdateCertifiedValues(CRM crm, UpdateCRMDto dto)
        {
            var existingById = crm.CertifiedValues.ToDictionary(v => v.Id, v => v);

            var newValues = new List<CRMValue>();

            foreach (var valueDto in dto.CertifiedValues)
            {
                if (valueDto.Id.HasValue && existingById.TryGetValue(valueDto.Id.Value, out var existing))
                {
                    existing.ElementId = valueDto.ElementId;
                    existing.Unit = valueDto.Unit ?? "ppm";
                    existing.CertifiedValue = valueDto.CertifiedValue;
                    existing.Uncertainty = valueDto.Uncertainty;
                    existing.MinAcceptable = valueDto.MinAcceptable;
                    existing.MaxAcceptable = valueDto.MaxAcceptable;
                    existing.IsActive = valueDto.IsActive;

                    newValues.Add(existing);
                }
                else
                {
                    var newValue = new CRMValue
                    {
                        Id = Guid.NewGuid(),
                        ElementId = valueDto.ElementId,
                        Unit = valueDto.Unit ?? "ppm",
                        CertifiedValue = valueDto.CertifiedValue,
                        Uncertainty = valueDto.Uncertainty,
                        MinAcceptable = valueDto.MinAcceptable,
                        MaxAcceptable = valueDto.MaxAcceptable,
                        IsActive = valueDto.IsActive
                    };

                    newValues.Add(newValue);
                }
            }

            crm.CertifiedValues = newValues;
        }

        #endregion
    }
}
