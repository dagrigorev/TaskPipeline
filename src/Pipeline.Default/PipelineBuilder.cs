using Pipeline.Models;

namespace Pipeline.Default;

/// <summary>
/// Fluent builder for sequential pipelines.
/// </summary>
public sealed class PipelineBuilder<TContext>
{
    private readonly List<PipelineStepDescriptor<TContext>> _steps = new();
    private readonly List<IPipelineBehavior<TContext>> _behaviors = new();

    public PipelineBuilder<TContext> AddStep(
        IPipelineStep<TContext> step,
        IPipelineCondition<TContext>? condition = null,
        int order = 0,
        bool continueOnError = false)
    {
        ArgumentNullException.ThrowIfNull(step);

        _steps.Add(new PipelineStepDescriptor<TContext>(
            Guid.NewGuid(),
            step.Name,
            order,
            step,
            condition,
            continueOnError));

        return this;
    }

    public PipelineBuilder<TContext> AddStep(
        string name,
        Action<TContext> action,
        Func<TContext, bool>? condition = null,
        int order = 0,
        bool continueOnError = false)
    {
        return AddStep(
            new DelegatePipelineStep<TContext>(name, action),
            condition is null ? null : new DelegateCondition<TContext>(condition),
            order,
            continueOnError);
    }

    public PipelineBuilder<TContext> AddStep(
        string name,
        Func<TContext, CancellationToken, ValueTask> action,
        Func<TContext, CancellationToken, ValueTask<bool>>? condition = null,
        int order = 0,
        bool continueOnError = false)
    {
        return AddStep(
            new DelegatePipelineStep<TContext>(name, action),
            condition is null ? null : new DelegateCondition<TContext>(condition),
            order,
            continueOnError);
    }

    public PipelineBuilder<TContext> AddBehavior(IPipelineBehavior<TContext> behavior)
    {
        _behaviors.Add(behavior ?? throw new ArgumentNullException(nameof(behavior)));
        return this;
    }

    public SequentialPipeline<TContext> Build()
        => new(_steps, _behaviors);
}
