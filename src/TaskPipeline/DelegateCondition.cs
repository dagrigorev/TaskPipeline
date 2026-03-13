namespace TaskPipeline;

using TaskPipeline.Abstractions;

/// <summary>
/// Condition implementation backed by a delegate.
/// </summary>
public sealed class DelegateCondition<TContext>(string name, Func<TContext, CancellationToken, ValueTask<bool>> predicate) : IPipelineCondition<TContext>
{
    private readonly Func<TContext, CancellationToken, ValueTask<bool>> _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));

    public string Name { get; } = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException("Condition name cannot be empty.", nameof(name)) : name;

    public ValueTask<bool> CanExecuteAsync(TContext context, CancellationToken cancellationToken) => _predicate(context, cancellationToken);
}
