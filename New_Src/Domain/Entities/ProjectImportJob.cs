using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

/// <summary>
/// Persists import job status so it survives restarts.
/// Primary key is JobId (Guid).
/// </summary>
public class ProjectImportJob
{
    [Key]
    public Guid JobId { get; set; }

    // Optional: the project that will be created/associated by import
    public Guid? ResultProjectId { get; set; }

    // The project name requested by the user (optional, useful for listing)
    public string? ProjectName { get; set; }

    // State stored as int (map to Shared.Models.ImportJobState)
    public int State { get; set; }

    public int TotalRows { get; set; }
    public int ProcessedRows { get; set; }
    public int Percent { get; set; }

    public string? Message { get; set; }

    // NEW: path to temporary uploaded file on disk (already existed)
    public string? TempFilePath { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // --- NEW fields for idempotency / retries ---
    // Optional operation identifier (useful when client provides an idempotency key)
    public Guid? OperationId { get; set; }

    // Number of attempts performed for this job
    public int Attempts { get; set; } = 0;

    // Last error message recorded for this job (if any)
    public string? LastError { get; set; }

    // When to try the next attempt (for backoff scheduling)
    public DateTime? NextAttemptAt { get; set; }
}