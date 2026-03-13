namespace TaskPipeline;

using TaskPipeline.Abstractions;

/// <summary>
/// Step implementation backed by a delegate.
/// </summary>
public sealed class DelegateStep<TContext>(string name, Func<TContext, CancellationToken, ValueTask> action) : IPipelineStep<TContext>
{
    private readonly Func<TContext, CancellationToken, ValueTask> _action = action ?? throw new ArgumentNullException(nameof(action));

    public string Name { get; } = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException("Step name cannot be empty.", nameof(name)) : name;

    public ValueTask ExecuteAsync(TContext context, CancellationToken cancellationToken) => _action(context, cancellationToken);
}
