using Core.Icp.Domain.Entities.Samples;
using Core.Icp.Domain.Enums;
using Core.Icp.Domain.Interfaces.Repositories;
using Infrastructure.Icp.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Icp.Data.Repositories
{
    public class SampleRepository : BaseRepository<Sample>, ISampleRepository
    {
        public SampleRepository(ICPDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Sample>> GetByProjectIdAsync(
            Guid projectId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(s => !s.IsDeleted && s.ProjectId == projectId)
                .OrderBy(s => s.RunDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Sample>> GetBySampleIdsAsync(
            IEnumerable<string> sampleIds,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(s => !s.IsDeleted && sampleIds.Contains(s.SampleId))
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Sample>> GetByStatusAsync(
            SampleStatus status,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(s => !s.IsDeleted && s.Status == status)
                .OrderBy(s => s.RunDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Sample>> GetByDateRangeAsync(
            DateTime startDate,
            DateTime endDate,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(s => !s.IsDeleted && s.RunDate >= startDate && s.RunDate <= endDate)
                .OrderBy(s => s.RunDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<Sample?> GetWithMeasurementsAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(s => s.Measurements)
                    .ThenInclude(m => m.Element)
                .Include(s => s.QualityChecks)
                .FirstOrDefaultAsync(s => !s.IsDeleted && s.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<Sample>> GetWithMeasurementsByProjectAsync(
            Guid projectId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(s => s.Measurements)
                    .ThenInclude(m => m.Element)
                .Include(s => s.QualityChecks)
                .Where(s => !s.IsDeleted && s.ProjectId == projectId)
                .OrderBy(s => s.RunDate)
                .ToListAsync(cancellationToken);
        }
    }
}