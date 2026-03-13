namespace Pipeline.Default;

/// <summary>
/// Delegate-backed execution condition.
/// </summary>
public sealed class DelegateCondition<TContext> : IPipelineCondition<TContext>
{
    private readonly Func<TContext, CancellationToken, ValueTask<bool>> _predicate;

    public DelegateCondition(Func<TContext, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        _predicate = (context, _) => ValueTask.FromResult(predicate(context));
    }

    public DelegateCondition(Func<TContext, CancellationToken, ValueTask<bool>> predicate)
    {
        _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
    }

    public ValueTask<bool> CanExecuteAsync(TContext context, CancellationToken cancellationToken = default)
        => _predicate(context, cancellationToken);
}
