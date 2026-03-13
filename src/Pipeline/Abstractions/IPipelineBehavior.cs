using Pipeline.Models;

namespace Pipeline;

/// <summary>
/// Intercepts pipeline step execution.
/// </summary>
public interface IPipelineBehavior<TContext>
{
    ValueTask OnStepStartingAsync(PipelineStepDescriptor<TContext> step, TContext context, CancellationToken cancellationToken = default);

    ValueTask OnStepCompletedAsync(PipelineStepDescriptor<TContext> step, TContext context, StepExecutionResult result, CancellationToken cancellationToken = default);
}
