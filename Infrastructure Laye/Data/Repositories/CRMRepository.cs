using Core.Icp.Domain.Entities.CRM;
using Core.Icp.Domain.Interfaces.Repositories;
using Infrastructure.Icp.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Icp.Data.Repositories
{
    public class CRMRepository : BaseRepository<CRM>, ICRMRepository
    {
        public CRMRepository(ICPDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<CRM>> GetActiveCRMsAsync(
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(c => !c.IsDeleted &&
                           (c.ExpirationDate == null || c.ExpirationDate > DateTime.UtcNow))
                .OrderBy(c => c.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<CRM?> GetByCRMIdAsync(
            string crmId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(c => !c.IsDeleted && c.CRMId == crmId, cancellationToken);
        }

        public async Task<CRM?> GetByNameAsync(
            string name,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(c => !c.IsDeleted && c.Name == name, cancellationToken);
        }

        public async Task<CRM?> GetWithCertifiedValuesAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(c => c.CertifiedValues)
                    .ThenInclude(v => v.Element)
                .FirstOrDefaultAsync(c => !c.IsDeleted && c.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<CRM>> GetByManufacturerAsync(
            string manufacturer,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(c => !c.IsDeleted && c.Manufacturer == manufacturer)
                .OrderBy(c => c.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<CRM>> GetExpiringCRMsAsync(
            DateTime beforeDate,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(c => !c.IsDeleted &&
                           c.ExpirationDate != null &&
                           c.ExpirationDate <= beforeDate)
                .OrderBy(c => c.ExpirationDate)
                .ToListAsync(cancellationToken);
        }
    }
}