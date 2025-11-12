using Core.Icp.Domain.Entities.QualityControl;
using Core.Icp.Domain.Enums;

namespace Core.Icp.Domain.Interfaces.Repositories
{
    public interface IQualityCheckRepository : IRepository<QualityCheck>
    {
        Task<IEnumerable<QualityCheck>> GetBySampleIdAsync(Guid sampleId, CancellationToken cancellationToken = default);
        Task<IEnumerable<QualityCheck>> GetByCheckTypeAsync(CheckType checkType, CancellationToken cancellationToken = default);
        Task<IEnumerable<QualityCheck>> GetFailedChecksAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<QualityCheck>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
    }
}