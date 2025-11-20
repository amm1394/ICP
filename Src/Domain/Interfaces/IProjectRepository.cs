using Core.Icp.Domain.Entities;

namespace Core.Icp.Domain.Interfaces;

public interface IProjectRepository
{
    Task<Project?> GetWithSamplesAsync(Guid projectId, CancellationToken ct = default);
    // دیگر متدها مثل Add, Update...
}