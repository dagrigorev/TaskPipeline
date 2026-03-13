namespace Pipeline;

/// <summary>
/// Legacy execution condition contract retained for backward compatibility.
/// Prefer <see cref="IPipelineCondition{TContext}"/> for new code.
/// </summary>
public interface IPipelineItemExecutionExpression
{
    bool CanExecute();
    bool CanExecute(params object[] args);
}
