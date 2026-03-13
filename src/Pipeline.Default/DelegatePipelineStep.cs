namespace Pipeline.Default;

/// <summary>
/// Delegate-backed pipeline step.
/// </summary>
public sealed class DelegatePipelineStep<TContext> : IPipelineStep<TContext>
{
    private readonly Func<TContext, CancellationToken, ValueTask> _action;

    public DelegatePipelineStep(string name, Action<TContext> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException("Step name cannot be empty.", nameof(name)) : name;
        _action = (context, _) =>
        {
            action(context);
            return ValueTask.CompletedTask;
        };
    }

    public DelegatePipelineStep(string name, Func<TContext, CancellationToken, ValueTask> action)
    {
        Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException("Step name cannot be empty.", nameof(name)) : name;
        _action = action ?? throw new ArgumentNullException(nameof(action));
    }

    public string Name { get; }

    public ValueTask ExecuteAsync(TContext context, CancellationToken cancellationToken = default)
        => _action(context, cancellationToken);
}
