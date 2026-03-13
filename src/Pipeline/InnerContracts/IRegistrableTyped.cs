namespace Pipeline.InnerContracts;

/// <summary>
/// Legacy typed registration contract retained for backward compatibility.
/// </summary>
public interface IRegistrableTyped : IRegistrable
{
    void Register<TOut>(IPipelineItem item);
    void Register<TIn, TOut>(IPipelineItem item);
    void Register<T1In, T2In, TOut>(IPipelineItem item);
    void Register<T1In, T2In, T3In, TOut>(IPipelineItem item);
    void Register<T1In, T2In, T3In, T4In, TOut>(IPipelineItem item);

    void UnRegister<TOut>(IPipelineItem item);
    void UnRegister<TIn, TOut>(IPipelineItem item);
    void UnRegister<T1In, T2In, TOut>(IPipelineItem item);
    void UnRegister<T1In, T2In, T3In, TOut>(IPipelineItem item);
    void UnRegister<T1In, T2In, T3In, T4In, TOut>(IPipelineItem item);
}
