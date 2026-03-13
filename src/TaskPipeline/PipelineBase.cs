namespace TaskPipeline;

using System.Diagnostics;
using TaskPipeline.Abstractions;

/// <summary>
/// Shared execution helpers for all pipeline nodes.
/// </summary>
internal abstract class PipelineNodeBase<TContext>(string name, NodeKind kind) : IPipelineNode<TContext>
{
    public string Name { get; } = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException("Node name cannot be empty.", nameof(name)) : name;

    public NodeKind Kind { get; } = kind;

    public ValueTask<NodeExecutionResult> ExecuteAsync(TContext context, CancellationToken cancellationToken = default)
        => ExecuteCoreAsync(context, cancellationToken);

    protected abstract ValueTask<NodeExecutionResult> ExecuteCoreAsync(TContext context, CancellationToken cancellationToken);

    protected static async ValueTask<NodeExecutionResult> ExecuteWithResultAsync(
        string name,
        NodeKind kind,
        Func<CancellationToken, ValueTask<(ExecutionStatus Status, Exception? Exception, IReadOnlyList<NodeExecutionResult> Children, IReadOnlyDictionary<string, string>? Metadata)>> action,
        CancellationToken cancellationToken)
    {
        var startedAt = DateTimeOffset.UtcNow;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var execution = await action(cancellationToken).ConfigureAwait(false);

            return NodeExecutionResult.Create(
                name,
                kind,
                execution.Status,
                startedAt,
                stopwatch.Elapsed,
                execution.Exception,
                execution.Metadata,
                execution.Children);
        }
        catch (OperationCanceledException ex)
        {
            return NodeExecutionResult.Create(name, kind, ExecutionStatus.Cancelled, startedAt, stopwatch.Elapsed, ex);
        }
        catch (Exception ex)
        {
            return NodeExecutionResult.Create(name, kind, ExecutionStatus.Failed, startedAt, stopwatch.Elapsed, ex);
        }
    }
}
