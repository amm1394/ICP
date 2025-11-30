using Application.DTOs;
using Shared.Wrapper;

namespace Application.Services;

/// <summary>
/// Service for drift correction and slope optimization
/// </summary>
public interface IDriftCorrectionService
{
    /// <summary>
    /// Analyze drift in the data
    /// </summary>
    Task<Result<DriftCorrectionResult>> AnalyzeDriftAsync(DriftCorrectionRequest request);

    /// <summary>
    /// Apply drift correction to the data
    /// </summary>
    Task<Result<DriftCorrectionResult>> ApplyDriftCorrectionAsync(DriftCorrectionRequest request);

    /// <summary>
    /// Detect segments based on standards (Cone/Base patterns)
    /// </summary>
    Task<Result<List<DriftSegment>>> DetectSegmentsAsync(Guid projectId, string? basePattern = null, string? conePattern = null);

    /// <summary>
    /// Calculate drift ratios between consecutive standards
    /// </summary>
    Task<Result<Dictionary<string, List<decimal>>>> CalculateDriftRatiosAsync(Guid projectId, List<string>? elements = null);

    /// <summary>
    /// Optimize slope for an element
    /// </summary>
    Task<Result<SlopeOptimizationResult>> OptimizeSlopeAsync(SlopeOptimizationRequest request);

    /// <summary>
    /// Zero the slope for an element (make it flat)
    /// </summary>
    Task<Result<SlopeOptimizationResult>> ZeroSlopeAsync(Guid projectId, string element);
}