using Domain.Entities;

namespace Application.Services;

public interface IChangeLogService
{
    Task LogChangeAsync(Guid projectId, string changeType, string? solutionLabel = null,
        string? element = null, string? oldValue = null, string? newValue = null,
        string? changedBy = null, string? details = null, Guid? batchId = null);

    Task LogBatchChangesAsync(Guid projectId, string changeType,
        IEnumerable<(string? SolutionLabel, string? Element, string? OldValue, string? NewValue)> changes,
        string? changedBy = null, string? details = null);

    Task<List<ChangeLog>> GetChangeLogAsync(Guid projectId, int page = 1, int pageSize = 50);

    Task<List<ChangeLog>> GetChangesByTypeAsync(Guid projectId, string changeType);

    Task<List<ChangeLog>> GetChangesBySampleAsync(Guid projectId, string solutionLabel);
}