using Domain.Entities;
using Shared.Wrapper;

namespace Application.Services;

/// <summary>
/// DTO for version tree node display
/// </summary>
public record VersionNodeDto(
    int StateId,
    int? ParentStateId,
    int VersionNumber,
    string ProcessingType,
    string? Description,
    DateTime Timestamp,
    bool IsActive,
    List<VersionNodeDto> Children
);

/// <summary>
/// DTO for creating a new version
/// </summary>
public record CreateVersionDto(
    Guid ProjectId,
    int? ParentStateId,
    string ProcessingType,
    string Data,
    string? Description
);

/// <summary>
/// Service interface for managing project versions (tree structure)
/// </summary>
public interface IVersionService
{
    /// <summary>
    /// Create a new version from a parent state
    /// </summary>
    Task<Result<ProjectState>> CreateVersionAsync(CreateVersionDto dto, CancellationToken ct = default);

    /// <summary>
    /// Get the version tree for a project
    /// </summary>
    Task<Result<List<VersionNodeDto>>> GetVersionTreeAsync(Guid projectId, CancellationToken ct = default);

    /// <summary>
    /// Get all versions for a project (flat list)
    /// </summary>
    Task<Result<List<ProjectState>>> GetAllVersionsAsync(Guid projectId, CancellationToken ct = default);

    /// <summary>
    /// Get the currently active version for a project
    /// </summary>
    Task<Result<ProjectState?>> GetActiveVersionAsync(Guid projectId, CancellationToken ct = default);

    /// <summary>
    /// Switch to a specific version (make it active)
    /// </summary>
    Task<Result<bool>> SwitchToVersionAsync(Guid projectId, int stateId, CancellationToken ct = default);

    /// <summary>
    /// Get a specific version by ID
    /// </summary>
    Task<Result<ProjectState?>> GetVersionAsync(int stateId, CancellationToken ct = default);

    /// <summary>
    /// Get version history path from root to a specific version
    /// </summary>
    Task<Result<List<ProjectState>>> GetVersionPathAsync(int stateId, CancellationToken ct = default);

    /// <summary>
    /// Delete a version and optionally its children
    /// </summary>
    Task<Result<bool>> DeleteVersionAsync(int stateId, bool deleteChildren = false, CancellationToken ct = default);

    /// <summary>
    /// Fork from an existing version (create branch)
    /// </summary>
    Task<Result<ProjectState>> ForkVersionAsync(int parentStateId, string processingType, string data, string? description = null, CancellationToken ct = default);
}
