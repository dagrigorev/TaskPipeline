namespace Pipeline.Models;

/// <summary>
/// Immutable metadata for a registered pipeline step.
/// </summary>
public sealed record PipelineStepDescriptor<TContext>(
    Guid Id,
    string Name,
    int Order,
    IPipelineStep<TContext> Step,
    IPipelineCondition<TContext>? Condition = null,
    bool ContinueOnError = false);
