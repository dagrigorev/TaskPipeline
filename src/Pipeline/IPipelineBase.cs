using Pipeline.InnerContracts;

namespace Pipeline;

/// <summary>
/// Legacy non-generic pipeline contract retained for backward compatibility.
/// Prefer <see cref="IPipeline{TContext}"/> for new code.
/// </summary>
public interface IPipelineBase : IRegistrable
{
    int Count { get; }
    void Execute();
    void Execute(params object[] args);
}
