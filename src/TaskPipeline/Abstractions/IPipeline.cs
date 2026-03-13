namespace TaskPipeline.Abstractions;

/// <summary>
/// Represents a strongly typed executable pipeline.
/// </summary>
/// <typeparam name="TContext">Pipeline context type.</typeparam>
public interface IPipeline<TContext>
{
    /// <summary>
    /// Gets the friendly pipeline name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Executes the pipeline for the provided context.
    /// </summary>
    /// <param name="context">Typed execution context.</param>
    /// <param name="cancellationToken">Cancellation token propagated through the entire graph.</param>
    ValueTask<PipelineExecutionResult> ExecuteAsync(TContext context, CancellationToken cancellationToken = default);
}
