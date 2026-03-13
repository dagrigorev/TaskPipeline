namespace TaskPipeline.Abstractions;

/// <summary>
/// Optional middleware-like hook around node execution.
/// </summary>
/// <typeparam name="TContext">Pipeline context type.</typeparam>
public interface IPipelineBehavior<TContext>
{
    /// <summary>
    /// Invokes the next behavior or the node itself.
    /// </summary>
    ValueTask<NodeExecutionResult> InvokeAsync(
        TContext context,
        PipelineNodeExecutionContext<TContext> nodeContext,
        Func<ValueTask<NodeExecutionResult>> next,
        CancellationToken cancellationToken);
}
