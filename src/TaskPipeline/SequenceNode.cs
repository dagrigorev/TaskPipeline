namespace TaskPipeline;

using TaskPipeline.Abstractions;

/// <summary>
/// Executes child nodes in declaration order.
/// </summary>
internal sealed class SequenceNode<TContext>(string name, IReadOnlyList<IPipelineNode<TContext>> children, PipelineOptions options) : PipelineNodeBase<TContext>(name, NodeKind.Sequence)
{
    private readonly IReadOnlyList<IPipelineNode<TContext>> _children = children ?? throw new ArgumentNullException(nameof(children));
    private readonly PipelineOptions _options = options ?? throw new ArgumentNullException(nameof(options));

    protected override ValueTask<NodeExecutionResult> ExecuteCoreAsync(TContext context, CancellationToken cancellationToken)
    {
        return ExecuteWithResultAsync(
            Name,
            Kind,
            async ct =>
            {
                var results = new List<NodeExecutionResult>(_children.Count);

                foreach (var child in _children)
                {
                    ct.ThrowIfCancellationRequested();
                    var childResult = await child.ExecuteAsync(context, ct).ConfigureAwait(false);
                    results.Add(childResult);

                    if (childResult.Status == ExecutionStatus.Cancelled)
                    {
                        break;
                    }

                    if (_options.FailureMode == PipelineFailureMode.FailFast && childResult.Status == ExecutionStatus.Failed)
                    {
                        break;
                    }
                }

                return (ResolveAggregateStatus(results), null, (IReadOnlyList<NodeExecutionResult>)results, (IReadOnlyDictionary<string, string>?)null);
            },
            cancellationToken);
    }

    private static ExecutionStatus ResolveAggregateStatus(IReadOnlyList<NodeExecutionResult> results)
    {
        if (results.Count == 0)
        {
            return ExecutionStatus.Skipped;
        }

        if (results.Any(result => result.Status == ExecutionStatus.Cancelled))
        {
            return ExecutionStatus.Cancelled;
        }

        if (results.Any(result => result.Status == ExecutionStatus.Failed))
        {
            return ExecutionStatus.Failed;
        }

        if (results.All(result => result.Status == ExecutionStatus.Skipped))
        {
            return ExecutionStatus.Skipped;
        }

        return ExecutionStatus.Success;
    }
}
