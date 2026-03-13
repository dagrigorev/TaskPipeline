namespace TaskPipeline;

using TaskPipeline.Abstractions;

/// <summary>
/// Example behavior that enriches results with a pipeline name.
/// </summary>
public sealed class MetadataBehavior<TContext> : IPipelineBehavior<TContext>
{
    public async ValueTask<NodeExecutionResult> InvokeAsync(
        TContext context,
        PipelineNodeExecutionContext<TContext> nodeContext,
        Func<ValueTask<NodeExecutionResult>> next,
        CancellationToken cancellationToken)
    {
        var result = await next().ConfigureAwait(false);
        var metadata = new Dictionary<string, string>(result.Metadata)
        {
            ["pipeline"] = nodeContext.PipelineName
        };

        return result with { Metadata = metadata };
    }
}
