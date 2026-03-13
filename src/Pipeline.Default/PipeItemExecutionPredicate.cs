namespace Pipeline.Default;

/// <summary>
/// Legacy predicate implementation retained for backward compatibility.
/// </summary>
public sealed class PipeItemExecutionPredicate : IPipelineItemExecutionExpression
{
    private readonly Func<bool>? _predicate;
    private readonly Func<object[], bool>? _argsPredicate;

    public PipeItemExecutionPredicate(Func<bool> predicate)
    {
        _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
    }

    public PipeItemExecutionPredicate(Func<object[], bool> predicate)
    {
        _argsPredicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
    }

    public bool CanExecute() => _predicate?.Invoke() ?? true;

    public bool CanExecute(params object[] args) => _argsPredicate?.Invoke(args) ?? _predicate?.Invoke() ?? true;
}
