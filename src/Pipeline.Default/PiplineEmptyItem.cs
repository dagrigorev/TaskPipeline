using Pipeline.Exceptions;

namespace Pipeline.Default;

/// <summary>
/// Legacy empty item implementation retained for backward compatibility.
/// </summary>
public sealed class PiplineEmptyItem : IPipelineItem
{
    private readonly List<Action> _continueActions = new();
    private readonly List<Action> _successActions = new();

    public PiplineEmptyItem()
    {
        Id = Guid.NewGuid();
    }

    public Guid Id { get; }

    public void ContinueWith(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        _continueActions.Add(action);
    }

    public void WhenSuccess(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        _successActions.Add(action);
    }

    public void WhenError(Action<PipelineItemExecutionException> action)
    {
        ArgumentNullException.ThrowIfNull(action);
    }

    public void Execute()
    {
        foreach (var action in _successActions)
        {
            action();
        }

        foreach (var action in _continueActions)
        {
            action();
        }
    }

    public void Execute(object[] args) => Execute();
}
