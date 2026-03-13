using Pipeline.Exceptions;

namespace Pipeline.Default;

/// <summary>
/// Legacy sequential pipeline retained for backward compatibility.
/// Internally uses an ordered list instead of a dictionary.
/// </summary>
public sealed class SequentialPipeline : IPipelineBase, Pipeline.InnerContracts.IPostActionable
{
    private readonly List<(IPipelineItemExecutionExpression Expression, IPipelineItem Item)> _items = new();
    private readonly List<Action> _continueActions = new();
    private readonly List<Action> _successActions = new();
    private readonly List<Action<PipelineItemExecutionException>> _errorActions = new();

    public int Count => _items.Count;

    public void Register(IPipelineItemExecutionExpression expression, IPipelineItem item)
    {
        ArgumentNullException.ThrowIfNull(expression);
        ArgumentNullException.ThrowIfNull(item);

        var existingIndex = _items.FindIndex(x => ReferenceEquals(x.Item, item));
        if (existingIndex >= 0)
        {
            _items[existingIndex] = (expression, item);
            return;
        }

        _items.Add((expression, item));
    }

    public void UnRegister(IPipelineItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        var removed = _items.RemoveAll(x => ReferenceEquals(x.Item, item));
        if (removed == 0)
        {
            throw new InvalidOperationException("The pipeline item is not registered.");
        }
    }

    public void Execute() => Execute(Array.Empty<object>());

    public void Execute(params object[] args)
    {
        foreach (var (expression, item) in _items)
        {
            try
            {
                if (!expression.CanExecute(args))
                {
                    continue;
                }

                item.Execute(args);
                foreach (var action in _successActions)
                {
                    action();
                }
            }
            catch (Exception ex)
            {
                var wrapped = PipelineItemExecutionException.Wrap(ex, item.Id, item.GetType().Name);
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
}
