namespace TaskPipeline;

using TaskPipeline.Abstractions;

/// <summary>
/// Chooses one branch based on a typed asynchronous predicate.
/// </summary>
internal sealed class ConditionalNode<TContext>(
    string name,
    IPipelineCondition<TContext> condition,
    IPipelineNode<TContext> whenTrue,
    IPipelineNode<TContext>? whenFalse) : PipelineNodeBase<TContext>(name, NodeKind.Conditional)
{
    private readonly IPipelineCondition<TContext> _condition = condition ?? throw new ArgumentNullException(nameof(condition));
    private readonly IPipelineNode<TContext> _whenTrue = whenTrue ?? throw new ArgumentNullException(nameof(whenTrue));
    private readonly IPipelineNode<TContext>? _whenFalse = whenFalse;

    protected override ValueTask<NodeExecutionResult> ExecuteCoreAsync(TContext context, CancellationToken cancellationToken)
    {
        return ExecuteWithResultAsync(
            Name,
            Kind,
            async ct =>
            {
                var canExecute = await _condition.CanExecuteAsync(context, ct).ConfigureAwait(false);
                var children = new List<NodeExecutionResult>(1);

                if (!canExecute)
                {
                    if (_whenFalse is null)
                    {
                        return (ExecutionStatus.Skipped, null, (IReadOnlyList<NodeExecutionResult>)children, new Dictionary<string, string>
                        {
                            ["condition"] = _condition.Name,
                            ["selected"] = "none"
                        });
                    }

                    var falseResult = await _whenFalse.ExecuteAsync(context, ct).ConfigureAwait(false);
                    children.Add(falseResult);

                    return (falseResult.Status, falseResult.Exception, (IReadOnlyList<NodeExecutionResult>)children, new Dictionary<string, string>
                    {
                        ["condition"] = _condition.Name,
                        ["selected"] = "false"
                    });
                }

                var trueResult = await _whenTrue.ExecuteAsync(context, ct).ConfigureAwait(false);
                children.Add(trueResult);

                return (trueResult.Status, trueResult.Exception, (IReadOnlyList<NodeExecutionResult>)children, new Dictionary<string, string>
                {
                    ["condition"] = _condition.Name,
                    ["selected"] = "true"
                });
            },
            cancellationToken);
    }
}
