using Core.Icp.Domain.Entities.Projects;
using Core.Icp.Domain.Enums;

namespace Core.Icp.Domain.Interfaces.Repositories
{
    public interface IProjectRepository : IRepository<Project>
    {
        Task<IEnumerable<Project>> GetByStatusAsync(ProjectStatus status, CancellationToken cancellationToken = default);
        Task<Project?> GetWithSamplesAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Project?> GetWithFullDetailsAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Project>> GetRecentProjectsAsync(int count, CancellationToken cancellationToken = default);
    }
}