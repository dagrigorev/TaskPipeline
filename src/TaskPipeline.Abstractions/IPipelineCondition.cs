namespace TaskPipeline.Abstractions;

/// <summary>
/// Represents a strongly typed asynchronous condition.
/// </summary>
/// <typeparam name="TContext">Pipeline context type.</typeparam>
public interface IPipelineCondition<in TContext>
{
    /// <summary>
    /// Gets the condition name used in diagnostics.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Evaluates whether the associated node should execute.
    /// </summary>
    ValueTask<bool> CanExecuteAsync(TContext context, CancellationToken cancellationToken);
}
