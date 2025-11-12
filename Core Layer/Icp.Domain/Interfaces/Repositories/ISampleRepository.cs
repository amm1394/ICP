using Core.Icp.Domain.Entities.Samples;
using Core.Icp.Domain.Enums;

namespace Core.Icp.Domain.Interfaces.Repositories
{
    public interface ISampleRepository : IRepository<Sample>
    {
        Task<IEnumerable<Sample>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Sample>> GetBySampleIdsAsync(IEnumerable<string> sampleIds, CancellationToken cancellationToken = default);
        Task<IEnumerable<Sample>> GetByStatusAsync(SampleStatus status, CancellationToken cancellationToken = default);
        Task<IEnumerable<Sample>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
        Task<Sample?> GetWithMeasurementsAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Sample>> GetWithMeasurementsByProjectAsync(Guid projectId, CancellationToken cancellationToken = default);
    }
}