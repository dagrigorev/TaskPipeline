using Pipeline.InnerContracts;

namespace Pipeline;

/// <summary>
/// Legacy pipeline item contract retained for backward compatibility.
/// Prefer <see cref="IPipelineStep{TContext}"/> for new code.
/// </summary>
public interface IPipelineItem : IPostActionable
{
    Guid Id { get; }
    void Execute();
    void Execute(object[] args);
}
