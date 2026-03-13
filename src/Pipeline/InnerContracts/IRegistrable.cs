namespace Pipeline.InnerContracts;

/// <summary>
/// Legacy registration contract retained for backward compatibility.
/// </summary>
public interface IRegistrable
{
    void Register(IPipelineItemExecutionExpression expression, IPipelineItem item);
    void UnRegister(IPipelineItem item);
}
