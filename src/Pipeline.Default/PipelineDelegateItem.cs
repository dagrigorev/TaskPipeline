using Pipeline.Exceptions;

namespace Pipeline.Default;

/// <summary>
/// Legacy delegate-backed item retained for backward compatibility.
/// </summary>
public sealed class PipelineDelegateItem : IPipelineItem
{
    private readonly Action? _delegateItem;
    private readonly Action<object[]>? _delegateArgsItem;
    private readonly List<Action> _continueActions = new();
    private readonly List<Action> _successActions = new();
    private readonly List<Action<PipelineItemExecutionException>> _errorActions = new();

    public PipelineDelegateItem(Action action)
    {
        _delegateItem = action ?? throw new ArgumentNullException(nameof(action));
        Id = Guid.NewGuid();
    }

    public PipelineDelegateItem(Action<object[]> action)
    {
        _delegateArgsItem = action ?? throw new ArgumentNullException(nameof(action));
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
        _errorActions.Add(action);
    }

    public void Execute()
    {
        try
        {
            if (_delegateItem is not null)
            {
                _delegateItem();
            }
            else
            {
                _delegateArgsItem?.Invoke(Array.Empty<object>());
            }

            foreach (var action in _successActions)
            {
                action();
            }
        }
        catch (Exception ex)
        {
            var wrapped = PipelineItemExecutionException.Wrap(ex, Id, nameof(PipelineDelegateItem));
            foreach (var action in _errorActions)
            {
                action(wrapped);
            }
        }
        finally
        {
            foreach (var action in _continueActions)
            {
                action();
            }
        }
    }

    public void Execute(object[] args)
    {
        try
        {
            if (_delegateArgsItem is not null)
            {
                _delegateArgsItem(args ?? Array.Empty<object>());
            }
            else
            {
                _delegateItem?.Invoke();
            }

            foreach (var action in _successActions)
            {
                action();
            }
        }
        catch (Exception ex)
        {
            var wrapped = PipelineItemExecutionException.Wrap(ex, Id, nameof(PipelineDelegateItem));
            foreach (var action in _errorActions)
            {
                action(wrapped);
            }
        }
        finally
        {
            foreach (var action in _continueActions)
            {
                action();
            }
        }
    }
}
