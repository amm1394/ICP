namespace Domain.Entities;

/// <summary>
/// Audit log for tracking all changes to project data
/// Equivalent to Python changes_log SQLite table
/// </summary>
public class ChangeLog
{
    public long Id { get; set; }

    public Guid ProjectId { get; set; }

    public string ChangeType { get; set; } = string.Empty; // Weight, Volume, DF, CRM, Drift, Optimization

    public string SolutionLabel { get; set; } = string.Empty;

    public string? Element { get; set; }

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public string? Details { get; set; } // JSON with additional info

    public string? UserId { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    // Navigation
    public Project? Project { get; set; }
}