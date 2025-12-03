using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

/// <summary>
/// Represents a version/snapshot of project data.
/// Supports tree structure: each state can have a parent (like Git commits).
/// This allows branching and returning to any previous state.
/// </summary>
public class ProjectState
{
    [Key]
    public int StateId { get; set; }

    public Guid ProjectId { get; set; }

    // Parent state for tree structure (null = root/initial import)
    public int? ParentStateId { get; set; }

    // Version number within this project (1, 2, 3, ...)
    public int VersionNumber { get; set; } = 1;

    // Type of processing that created this state
    public string ProcessingType { get; set; } = "Import";

    // Full serialized project state (JSON)
    public string Data { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    // Optional description (e.g. "autosave", "manual save", "Drift Correction Applied")
    public string? Description { get; set; }

    // Is this the currently active version?
    public bool IsActive { get; set; } = false;

    // Navigation
    public Project? Project { get; set; }
    public ProjectState? ParentState { get; set; }
    public ICollection<ProjectState> ChildStates { get; set; } = new List<ProjectState>();
}

/// <summary>
/// Enum-like constants for ProcessingType
/// </summary>
public static class ProcessingTypes
{
    public const string Import = "Import";
    public const string WeightCorrection = "Weight Correction";
    public const string VolumeCorrection = "Volume Correction";
    public const string DfCorrection = "DF Correction";
    public const string DriftCorrection = "Drift Correction";
    public const string CrmCheck = "CRM Check";
    public const string RmCheck = "RM Check";
    public const string EmptyRowRemoval = "Empty Row Removal";
    public const string ManualEdit = "Manual Edit";
    public const string Optimization = "Optimization";
}