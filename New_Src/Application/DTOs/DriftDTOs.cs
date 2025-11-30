namespace Application.DTOs;

/// <summary>
/// Request for drift correction analysis
/// </summary>
public record DriftCorrectionRequest(
    Guid ProjectId,
    List<string>? SelectedElements = null,
    DriftMethod Method = DriftMethod.Linear,
    bool UseSegmentation = true,
    string? BasePattern = null,
    string? ConePattern = null
);

/// <summary>
/// Drift correction method
/// </summary>
public enum DriftMethod
{
    /// <summary>No drift correction</summary>
    None,

    /// <summary>Linear interpolation between standards</summary>
    Linear,

    /// <summary>Stepwise correction (apply at each standard)</summary>
    Stepwise,

    /// <summary>Polynomial fit</summary>
    Polynomial
}

/// <summary>
/// Result of drift correction
/// </summary>
public record DriftCorrectionResult(
    int TotalSamples,
    int CorrectedSamples,
    int SegmentsFound,
    List<DriftSegment> Segments,
    Dictionary<string, ElementDriftInfo> ElementDrifts,
    List<CorrectedSampleDto> CorrectedData
);

/// <summary>
/// A segment between two standards
/// </summary>
public record DriftSegment(
    int SegmentIndex,
    int StartIndex,
    int EndIndex,
    string? StartStandard,
    string? EndStandard,
    int SampleCount
);

/// <summary>
/// Drift information for an element
/// </summary>
public record ElementDriftInfo(
    string Element,
    decimal InitialRatio,
    decimal FinalRatio,
    decimal DriftPercent,
    decimal Slope,
    decimal Intercept
);

/// <summary>
/// Corrected sample data
/// </summary>
public record CorrectedSampleDto(
    string SolutionLabel,
    int OriginalIndex,
    int SegmentIndex,
    Dictionary<string, decimal?> OriginalValues,
    Dictionary<string, decimal?> CorrectedValues,
    Dictionary<string, decimal> CorrectionFactors
);

/// <summary>
/// Request for slope optimization
/// </summary>
public record SlopeOptimizationRequest(
    Guid ProjectId,
    string Element,
    SlopeAction Action,
    decimal? TargetSlope = null
);

/// <summary>
/// Slope optimization actions
/// </summary>
public enum SlopeAction
{
    ZeroSlope,
    RotateUp,
    RotateDown,
    SetCustom
}

/// <summary>
/// Result of slope optimization
/// </summary>
public record SlopeOptimizationResult(
    string Element,
    decimal OriginalSlope,
    decimal NewSlope,
    decimal OriginalIntercept,
    decimal NewIntercept,
    List<CorrectedSampleDto> CorrectedData
);