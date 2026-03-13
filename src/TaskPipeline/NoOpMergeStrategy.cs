namespace TaskPipeline;

using TaskPipeline.Abstractions;

/// <summary>
/// Default merge strategy. It intentionally does nothing when branches already modify a shared context.
/// </summary>
internal sealed class NoOpMergeStrategy<TContext> : IBranchMergeStrategy<TContext>
{
    public static NoOpMergeStrategy<TContext> Instance { get; } = new();

    public string Name => "no-op";

    public ValueTask MergeAsync(TContext context, IReadOnlyList<NodeExecutionResult> branchResults, CancellationToken cancellationToken)
        => ValueTask.CompletedTask;
}
