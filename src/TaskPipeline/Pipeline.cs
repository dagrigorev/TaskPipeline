namespace TaskPipeline;

using System.Diagnostics;
using TaskPipeline.Abstractions;

/// <summary>
/// Production-oriented strongly typed pipeline implementation.
/// </summary>
public sealed class Pipeline<TContext> : IPipeline<TContext>
{
    private readonly IPipelineNode<TContext> _root;
    private readonly IReadOnlyList<IPipelineBehavior<TContext>> _behaviors;

    internal Pipeline(string name, IPipelineNode<TContext> root, PipelineOptions options, IReadOnlyList<IPipelineBehavior<TContext>> behaviors)
    {
        Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException("Pipeline name cannot be empty.", nameof(name)) : name;
        _root = root ?? throw new ArgumentNullException(nameof(root));
        Options = options ?? throw new ArgumentNullException(nameof(options));
        _behaviors = behaviors ?? throw new ArgumentNullException(nameof(behaviors));
    }

    public string Name { get; }

    public PipelineOptions Options { get; }

    public async ValueTask<PipelineExecutionResult> ExecuteAsync(TContext context, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var startedAt = DateTimeOffset.UtcNow;

        NodeExecutionResult root;
        try
        {
            root = await ExecuteWithBehaviorsAsync(_root, context, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException ex)
        {
            root = NodeExecutionResult.Create(Name, NodeKind.Pipeline, ExecutionStatus.Cancelled, startedAt, stopwatch.Elapsed, ex);
        }

        return new PipelineExecutionResult
        {
            Name = Name,
            Status = root.Status,
            Duration = stopwatch.Elapsed,
            Root = root.Kind == NodeKind.Pipeline
                ? root
                : root with { Name = Name, Kind = NodeKind.Pipeline }
        };
    }

    private ValueTask<NodeExecutionResult> ExecuteWithBehaviorsAsync(IPipelineNode<TContext> node, TContext context, CancellationToken cancellationToken)
    {
        var nodeContext = new PipelineNodeExecutionContext<TContext>(Name, node.Name, node.Kind, context);

        Func<ValueTask<NodeExecutionResult>> next = () => node.ExecuteAsync(context, cancellationToken);

        foreach (var behavior in _behaviors.Reverse())
        {
            var current = next;
            next = () => behavior.InvokeAsync(context, nodeContext, current, cancellationToken);
        }

        return next();
    }
}
