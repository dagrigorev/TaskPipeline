namespace TaskPipeline;

using TaskPipeline.Abstractions;

/// <summary>
/// Leaf node that executes a single pipeline step.
/// </summary>
internal sealed class StepNode<TContext>(IPipelineStep<TContext> step) : PipelineNodeBase<TContext>(step.Name, NodeKind.Step)
{
    private readonly IPipelineStep<TContext> _step = step ?? throw new ArgumentNullException(nameof(step));

    protected override ValueTask<NodeExecutionResult> ExecuteCoreAsync(TContext context, CancellationToken cancellationToken)
    {
        return ExecuteWithResultAsync(
            Name,
            Kind,
            async ct =>
            {
                await _step.ExecuteAsync(context, ct).ConfigureAwait(false);
                return (ExecutionStatus.Success, null, Array.Empty<NodeExecutionResult>(), (IReadOnlyDictionary<string, string>?)null);
            },
            cancellationToken);
    }
}
