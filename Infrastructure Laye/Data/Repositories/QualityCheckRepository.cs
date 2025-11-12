using Core.Icp.Domain.Entities.QualityControl;
using Core.Icp.Domain.Enums;
using Core.Icp.Domain.Interfaces.Repositories;
using Infrastructure.Icp.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Icp.Data.Repositories
{
    public class QualityCheckRepository : BaseRepository<QualityCheck>, IQualityCheckRepository
    {
        public QualityCheckRepository(ICPDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<QualityCheck>> GetBySampleIdAsync(
            Guid sampleId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(q => !q.IsDeleted && q.SampleId == sampleId)
                .OrderBy(q => q.CheckType)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<QualityCheck>> GetByCheckTypeAsync(
            CheckType checkType,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(q => !q.IsDeleted && q.CheckType == checkType)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<QualityCheck>> GetFailedChecksAsync(
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(q => q.Sample)
                .Where(q => !q.IsDeleted && q.Status == CheckStatus.Fail)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<QualityCheck>> GetByProjectIdAsync(
            Guid projectId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(q => q.Sample)
                .Where(q => !q.IsDeleted && q.Sample.ProjectId == projectId)
                .OrderBy(q => q.CheckType)
                .ToListAsync(cancellationToken);
        }
    }
}