using Shared.Wrapper;
using System.Collections.Concurrent;

namespace Application.Services;

// In-memory implementation (lightweight) for development / testing.
// This implementation DOES NOT depend on EF or SQL Server.
public class ProjectPersistenceService : IProjectPersistenceService
{
    private readonly ConcurrentDictionary<Guid, StoredProject> _store = new();

    private class StoredProject
    {
        public Guid ProjectId { get; init; }
        public string ProjectName { get; set; } = string.Empty;
        public string? Owner { get; set; }
        public DateTime CreatedAt { get; init; }
        public DateTime LastModifiedAt { get; set; }
        public List<RawDataDto> RawRows { get; } = new();
        public List<(DateTime Timestamp, string Data, string? Description)> States { get; } = new();
    }

    public Task<Result<ProjectSaveResult>> SaveProjectAsync(Guid projectId, string projectName, string? owner, List<RawDataDto>? rawRows, string? stateJson)
    {
        try
        {
            var now = DateTime.UtcNow;
            var id = projectId == Guid.Empty ? Guid.NewGuid() : projectId;

            var stored = _store.GetOrAdd(id, _ => new StoredProject
            {
                ProjectId = id,
                ProjectName = projectName,
                Owner = owner,
                CreatedAt = now,
                LastModifiedAt = now
            });

            // update metadata
            stored.ProjectName = projectName;
            stored.Owner = owner;
            stored.LastModifiedAt = now;

            // append raw rows (caller semantics: append). If replace semantics desired, clear first.
            if (rawRows != null && rawRows.Count > 0)
            {
                stored.RawRows.AddRange(rawRows);
            }

            if (!string.IsNullOrEmpty(stateJson))
            {
                stored.States.Add((now, stateJson, "ManualSave"));
            }

            var result = new ProjectSaveResult(stored.ProjectId);
            return Task.FromResult(Result<ProjectSaveResult>.Success(result));
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result<ProjectSaveResult>.Fail($"Save failed: {ex.Message}"));
        }
    }

    public Task<Result<ProjectLoadDto>> LoadProjectAsync(Guid projectId)
    {
        try
        {
            if (!_store.TryGetValue(projectId, out var stored))
                return Task.FromResult(Result<ProjectLoadDto>.Fail("Project not found."));

            var latestState = stored.States.OrderByDescending(s => s.Timestamp).FirstOrDefault().Data;

            var dto = new ProjectLoadDto(
                stored.ProjectId,
                stored.ProjectName,
                stored.CreatedAt,
                stored.LastModifiedAt,
                stored.Owner,
                stored.RawRows.ToList(),
                latestState
            );

            return Task.FromResult(Result<ProjectLoadDto>.Success(dto));
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result<ProjectLoadDto>.Fail($"Load failed: {ex.Message}"));
        }
    }
}