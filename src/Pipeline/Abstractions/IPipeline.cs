using Pipeline.Models;

namespace Pipeline;

/// <summary>
/// Executes a pipeline for the provided context.
/// </summary>
public interface IPipeline<TContext>
{
    int Count { get; }

    ValueTask<PipelineExecutionResult> ExecuteAsync(
        TContext context,
        CancellationToken cancellationToken = default);
}
