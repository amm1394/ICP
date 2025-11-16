using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Icp.Domain.Models.Projects;

namespace Core.Icp.Domain.Interfaces.Services
{
    public interface IProjectQueryService
    {
        Task<PagedProjectListResult> GetPagedAsync(
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);

        Task<ProjectDetailsResult?> GetDetailsAsync(
            Guid id,
            CancellationToken cancellationToken = default);
    }
}
