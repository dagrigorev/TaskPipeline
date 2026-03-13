namespace TaskPipeline.Abstractions;

/// <summary>
/// Represents a single strongly typed step.
/// </summary>
/// <typeparam name="TContext">Pipeline context type.</typeparam>
public interface IPipelineStep<in TContext>
{
    /// <summary>
    /// Gets the step name used in diagnostics and execution results.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Executes the step.
    /// </summary>
    ValueTask ExecuteAsync(TContext context, CancellationToken cancellationToken);
}
