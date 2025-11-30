using System.Text.Json.Serialization;

namespace Application.DTOs;

/// <summary>
/// Request for Blank & Scale optimization
/// </summary>
public record BlankScaleOptimizationRequest(
    Guid ProjectId,
    List<string>? Elements = null,
    decimal MinDiffPercent = -10m,
    decimal MaxDiffPercent = 10m,
    int MaxIterations = 100,
    int PopulationSize = 50
);

/// <summary>
/// Result of Blank & Scale optimization
/// </summary>
public record BlankScaleOptimizationResult(
    int TotalSamples,
    int PassedBefore,
    int PassedAfter,
    decimal ImprovementPercent,
    Dictionary<string, ElementOptimization> ElementOptimizations,
    List<OptimizedSampleDto> OptimizedData
);

/// <summary>
/// Optimization result for a single element
/// </summary>
public record ElementOptimization(
    string Element,
    decimal OptimalBlank,
    decimal OptimalScale,
    int PassedBefore,
    int PassedAfter,
    decimal MeanDiffBefore,
    decimal MeanDiffAfter
);

/// <summary>
/// Optimized sample data
/// </summary>
public record OptimizedSampleDto(
    string SolutionLabel,
    string CrmId,
    Dictionary<string, decimal?> OriginalValues,
    Dictionary<string, decimal?> CrmValues,
    Dictionary<string, decimal?> OptimizedValues,
    Dictionary<string, decimal> DiffPercentBefore,
    Dictionary<string, decimal> DiffPercentAfter,
    Dictionary<string, bool> PassStatusBefore,
    Dictionary<string, bool> PassStatusAfter
);

/// <summary>
/// Manual Blank & Scale adjustment request
/// </summary>
public record ManualBlankScaleRequest(
    Guid ProjectId,
    string Element,
    decimal Blank,
    decimal Scale
);

/// <summary>
/// Result of manual adjustment
/// </summary>
public record ManualBlankScaleResult(
    string Element,
    decimal Blank,
    decimal Scale,
    int PassedBefore,
    int PassedAfter,
    List<OptimizedSampleDto> OptimizedData
);