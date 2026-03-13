using Pipeline.Exceptions;

namespace Pipeline.Models;

/// <summary>
/// Result of a single pipeline step.
/// </summary>
public sealed record StepExecutionResult(
    Guid StepId,
    string StepName,
    StepExecutionStatus Status,
    TimeSpan Duration,
    PipelineItemExecutionException? Exception = null);
