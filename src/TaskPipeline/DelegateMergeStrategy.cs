namespace TaskPipeline;

using TaskPipeline.Abstractions;

/// <summary>
/// Merge strategy backed by a delegate.
/// </summary>
public sealed class DelegateMergeStrategy<TContext>(string name, Func<TContext, IReadOnlyList<NodeExecutionResult>, CancellationToken, ValueTask> action) : IBranchMergeStrategy<TContext>
{
    private readonly Func<TContext, IReadOnlyList<NodeExecutionResult>, CancellationToken, ValueTask> _action = action ?? throw new ArgumentNullException(nameof(action));

    public string Name { get; } = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException("Merge strategy name cannot be empty.", nameof(name)) : name;

    public ValueTask MergeAsync(TContext context, IReadOnlyList<NodeExecutionResult> branchResults, CancellationToken cancellationToken)
        => _action(context, branchResults, cancellationToken);
}
