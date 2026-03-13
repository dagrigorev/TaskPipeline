using System.Diagnostics;
using Pipeline.Exceptions;
using Pipeline.Models;

namespace Pipeline.Default;

/// <summary>
/// Modern sequential pipeline implementation.
/// </summary>
public sealed class SequentialPipeline<TContext> : IPipeline<TContext>
{
    private readonly IReadOnlyList<PipelineStepDescriptor<TContext>> _steps;
    private readonly IReadOnlyList<IPipelineBehavior<TContext>> _behaviors;

    public SequentialPipeline(
        IEnumerable<PipelineStepDescriptor<TContext>> steps,
        IEnumerable<IPipelineBehavior<TContext>>? behaviors = null)
    {
        ArgumentNullException.ThrowIfNull(steps);
        _steps = steps.OrderBy(static x => x.Order).ToArray();
        _behaviors = behaviors?.ToArray() ?? Array.Empty<IPipelineBehavior<TContext>>();
    }

    public int Count => _steps.Count;

    public async ValueTask<PipelineExecutionResult> ExecuteAsync(TContext context, CancellationToken cancellationToken = default)
    {
        var results = new List<StepExecutionResult>(_steps.Count);
        var hadFailures = false;
        var hadSuccesses = false;

        foreach (var step in _steps)
        {
            cancellationToken.ThrowIfCancellationRequested();

            foreach (var behavior in _behaviors)
            {
                await behavior.OnStepStartingAsync(step, context, cancellationToken).ConfigureAwait(false);
            }

            var sw = Stopwatch.StartNew();
            StepExecutionResult result;

            try
            {
                if (step.Condition is not null)
                {
                    var canExecute = await step.Condition.CanExecuteAsync(context, cancellationToken).ConfigureAwait(false);
                    if (!canExecute)
                    {
                        result = new StepExecutionResult(step.Id, step.Name, StepExecutionStatus.Skipped, sw.Elapsed);
                        results.Add(result);
                        await NotifyStepCompletedAsync(step, context, result, cancellationToken).ConfigureAwait(false);
                        continue;
                    }
                }

                await step.Step.ExecuteAsync(context, cancellationToken).ConfigureAwait(false);
                hadSuccesses = true;
                result = new StepExecutionResult(step.Id, step.Name, StepExecutionStatus.Succeeded, sw.Elapsed);
            }
            catch (OperationCanceledException)
            {
                result = new StepExecutionResult(step.Id, step.Name, StepExecutionStatus.Cancelled, sw.Elapsed);
                results.Add(result);
                await NotifyStepCompletedAsync(step, context, result, cancellationToken).ConfigureAwait(false);
                return new PipelineExecutionResult
                {
                    Status = PipelineExecutionStatus.Cancelled,
                    Steps = results
                };
            }
            catch (Exception ex)
            {
                hadFailures = true;
                result = new StepExecutionResult(
                    step.Id,
                    step.Name,
                    StepExecutionStatus.Failed,
                    sw.Elapsed,
                    PipelineItemExecutionException.Wrap(ex, step.Id, step.Name));

                if (!step.ContinueOnError)
                {
                    results.Add(result);
                    await NotifyStepCompletedAsync(step, context, result, cancellationToken).ConfigureAwait(false);
                    return new PipelineExecutionResult
                    {
                        Status = hadSuccesses ? PipelineExecutionStatus.PartialSuccess : PipelineExecutionStatus.Failed,
                        Steps = results
                    };
                }
            }

            results.Add(result);
            await NotifyStepCompletedAsync(step, context, result, cancellationToken).ConfigureAwait(false);
        }

        return new PipelineExecutionResult
        {
            Status = hadFailures && hadSuccesses
                ? PipelineExecutionStatus.PartialSuccess
                : hadFailures
                    ? PipelineExecutionStatus.Failed
                    : PipelineExecutionStatus.Succeeded,
            Steps = results
        };
    }

    private async ValueTask NotifyStepCompletedAsync(
        PipelineStepDescriptor<TContext> step,
        TContext context,
        StepExecutionResult result,
        CancellationToken cancellationToken)
    {
        foreach (var behavior in _behaviors)
        {
            await behavior.OnStepCompletedAsync(step, context, result, cancellationToken).ConfigureAwait(false);
        }
    }
}
