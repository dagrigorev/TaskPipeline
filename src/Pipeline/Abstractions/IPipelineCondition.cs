namespace Pipeline;

/// <summary>
/// Determines whether a step can execute for a given context.
/// </summary>
public interface IPipelineCondition<TContext>
{
    ValueTask<bool> CanExecuteAsync(TContext context, CancellationToken cancellationToken = default);
}
