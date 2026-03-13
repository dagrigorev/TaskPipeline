using Pipeline.Exceptions;

namespace Pipeline.InnerContracts;

/// <summary>
/// Legacy callback contract retained for backward compatibility.
/// </summary>
public interface IPostActionable
{
    void ContinueWith(Action action);
    void WhenSuccess(Action action);
    void WhenError(Action<PipelineItemExecutionException> action);
}
