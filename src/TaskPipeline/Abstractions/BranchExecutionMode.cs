namespace TaskPipeline.Abstractions;

/// <summary>
/// Defines whether branches are executed one-by-one or concurrently.
/// </summary>
public enum BranchExecutionMode
{
    Sequential,
    Parallel
}
