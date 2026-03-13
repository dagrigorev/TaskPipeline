using NUnit.Framework;
using Pipeline.Default;
using Pipeline.Models;

namespace TestPipeline;

public class TestsModernPipeline
{
    private sealed class SampleContext
    {
        public int Counter { get; set; }
        public List<string> Log { get; } = new();
    }

    [Test]
    public async Task Modern_pipeline_executes_steps_in_order()
    {
        var context = new SampleContext();
        var pipeline = new PipelineBuilder<SampleContext>()
            .AddStep("first", ctx => ctx.Log.Add("1"), order: 2)
            .AddStep("second", ctx => ctx.Log.Add("0"), order: 1)
            .Build();

        var result = await pipeline.ExecuteAsync(context);

        Assert.That(result.Status, Is.EqualTo(PipelineExecutionStatus.Succeeded));
        Assert.That(context.Log, Is.EqualTo(new[] { "0", "1" }));
    }

    [Test]
    public async Task Modern_pipeline_skips_step_when_condition_fails()
    {
        var context = new SampleContext();
        var pipeline = new PipelineBuilder<SampleContext>()
            .AddStep("skip-me", ctx => ctx.Counter++, condition: _ => false)
            .Build();

        var result = await pipeline.ExecuteAsync(context);

        Assert.That(context.Counter, Is.EqualTo(0));
        Assert.That(result.SkippedCount, Is.EqualTo(1));
    }

    [Test]
    public async Task Modern_pipeline_returns_partial_success_when_continue_on_error_is_enabled()
    {
        var context = new SampleContext();
        var pipeline = new PipelineBuilder<SampleContext>()
            .AddStep("boom", _ => throw new InvalidOperationException("fail"), continueOnError: true)
            .AddStep("recover", ctx => ctx.Counter++)
            .Build();

        var result = await pipeline.ExecuteAsync(context);

        Assert.That(result.Status, Is.EqualTo(PipelineExecutionStatus.PartialSuccess));
        Assert.That(result.FailedCount, Is.EqualTo(1));
        Assert.That(result.SucceededCount, Is.EqualTo(1));
        Assert.That(context.Counter, Is.EqualTo(1));
    }

    [Test]
    public async Task Modern_pipeline_stops_on_failure_by_default()
    {
        var context = new SampleContext();
        var pipeline = new PipelineBuilder<SampleContext>()
            .AddStep("boom", _ => throw new InvalidOperationException("fail"))
            .AddStep("never", ctx => ctx.Counter++)
            .Build();

        var result = await pipeline.ExecuteAsync(context);

        Assert.That(result.Status, Is.EqualTo(PipelineExecutionStatus.Failed));
        Assert.That(result.Steps.Count, Is.EqualTo(1));
        Assert.That(context.Counter, Is.EqualTo(0));
    }

    [Test]
    public void Modern_pipeline_honors_cancellation()
    {
        var context = new SampleContext();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var pipeline = new PipelineBuilder<SampleContext>()
            .AddStep("noop", static _ => { })
            .Build();

        Assert.ThrowsAsync<OperationCanceledException>(async () => await pipeline.ExecuteAsync(context, cts.Token));
    }
}
