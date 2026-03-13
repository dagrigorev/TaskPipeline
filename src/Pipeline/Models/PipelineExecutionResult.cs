namespace Pipeline.Models;

/// <summary>
/// Aggregate pipeline execution result.
/// </summary>
public sealed class PipelineExecutionResult
{
    public PipelineExecutionStatus Status { get; init; }

    public IReadOnlyList<StepExecutionResult> Steps { get; init; } = Array.Empty<StepExecutionResult>();

    public bool IsSuccess => Status is PipelineExecutionStatus.Succeeded or PipelineExecutionStatus.PartialSuccess;

    public int SucceededCount => Steps.Count(static x => x.Status == StepExecutionStatus.Succeeded);
    public int FailedCount => Steps.Count(static x => x.Status == StepExecutionStatus.Failed);
    public int SkippedCount => Steps.Count(static x => x.Status == StepExecutionStatus.Skipped);
}
