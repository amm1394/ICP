using Domain.Entities;

namespace Infrastructure.Services;

public interface IChangeLogService
{
    Task LogChangeAsync(Guid projectId, string changeType, string solutionLabel,
        string? element, object? oldValue, object? newValue, string? details = null, string? userId = null);

    Task<List<ChangeLog>> GetChangesAsync(Guid projectId, string? changeType = null, int limit = 100);

    Task<List<ChangeLog>> GetChangesBySampleAsync(Guid projectId, string solutionLabel);
}
