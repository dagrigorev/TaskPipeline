namespace TaskPipeline.Abstractions;

/// <summary>
/// Represents the final state of a pipeline node execution.
/// </summary>
public enum ExecutionStatus
{
    Success,
    Skipped,
    Failed,
    Cancelled
}
