namespace TaskPipeline.Abstractions;

/// <summary>
/// Provides a hook that runs after branch execution and before the pipeline continues.
/// </summary>
/// <typeparam name="TContext">Pipeline context type.</typeparam>
public interface IBranchMergeStrategy<in TContext>
{
    /// <summary>
    /// Gets the merge strategy name used in diagnostics.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Merges branch outcomes back into the shared context.
    /// </summary>
    ValueTask MergeAsync(TContext context, IReadOnlyList<NodeExecutionResult> branchResults, CancellationToken cancellationToken);
}
