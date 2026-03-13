namespace Pipeline;

/// <summary>
/// A single executable step inside a pipeline.
/// </summary>
public interface IPipelineStep<TContext>
{
    string Name { get; }

    ValueTask ExecuteAsync(TContext context, CancellationToken cancellationToken = default);
}
