namespace TaskPipeline.Tests;

using TaskPipeline.Abstractions;
using Xunit;

/// <summary>
/// Execution tests
/// </summary>
public sealed class PipelineExecutionTests
{
    [Fact]
    public async Task ExecutesStepsSequentiallyInDeclarationOrder()
    {
        var context = new TestContext();
        var pipeline = PipelineBuilder<TestContext>
            .Create("sequential")
            .AddStep("step-1", ctx => ctx.Events.Enqueue("step-1"))
            .AddStep("step-2", ctx => ctx.Events.Enqueue("step-2"))
            .AddStep("step-3", ctx => ctx.Events.Enqueue("step-3"))
            .Build();

        var result = await pipeline.ExecuteAsync(context);

        Assert.Equal(ExecutionStatus.Success, result.Status);
        Assert.Equal(["step-1", "step-2", "step-3"], context.Events.ToArray());
        Assert.Equal(["step-1", "step-2", "step-3"], result.Root.Children.Select(child => child.Name).ToArray());
    }

    [Fact]
    public async Task ExecutesTrueConditionalBranchAndSkipsFalseBranch()
    {
        var context = new TestContext { Condition = true };
        var pipeline = PipelineBuilder<TestContext>
            .Create("conditional")
            .AddConditional(
                "choose-path",
                (ctx, _) => ValueTask.FromResult(ctx.Condition),
                whenTrue: branch => branch.AddStep("true-step", ctx => ctx.Events.Enqueue("true-step")),
                whenFalse: branch => branch.AddStep("false-step", ctx => ctx.Events.Enqueue("false-step")))
            .Build();

        var result = await pipeline.ExecuteAsync(context);
        var conditionalResult = Assert.Single(result.Root.Children);

        Assert.Equal(ExecutionStatus.Success, result.Status);
        Assert.Equal(["true-step"], context.Events.ToArray());
        Assert.Equal("true", conditionalResult.Metadata["selected"]);
    }

    [Fact]
    public async Task SkipsConditionalNodeWhenConditionIsFalseAndNoElseBranchExists()
    {
        var context = new TestContext { Condition = false };
        var pipeline = PipelineBuilder<TestContext>
            .Create("conditional-skip")
            .AddConditional(
                "choose-path",
                (ctx, _) => ValueTask.FromResult(ctx.Condition),
                whenTrue: branch => branch.AddStep("true-step", ctx => ctx.Events.Enqueue("true-step")))
            .Build();

        var result = await pipeline.ExecuteAsync(context);
        var conditionalResult = Assert.Single(result.Root.Children);

        Assert.Equal(ExecutionStatus.Skipped, result.Status);
        Assert.Empty(context.Events);
        Assert.Equal(ExecutionStatus.Skipped, conditionalResult.Status);
    }

    [Fact]
    public async Task AggregatesParallelBranchResultsAndRunsMergeStrategy()
    {
        var context = new TestContext();
        var pipeline = PipelineBuilder<TestContext>
            .Create("fork")
            .AddFork(
                "parallel-work",
                fork => fork
                    .AddBranch("branch-a", branch => branch.AddStep("a1", ctx => ctx.BranchSum += 2))
                    .AddBranch("branch-b", branch => branch.AddStep("b1", ctx => ctx.BranchSum += 3)),
                executionMode: BranchExecutionMode.Parallel,
                mergeStrategy: new DelegateMergeStrategy<TestContext>(
                    "record-merge",
                    (ctx, results, _) =>
                    {
                        ctx.Events.Enqueue($"merged:{results.Count}");
                        return ValueTask.CompletedTask;
                    }))
            .Build();

        var result = await pipeline.ExecuteAsync(context);
        var forkResult = Assert.Single(result.Root.Children);

        Assert.Equal(ExecutionStatus.Success, result.Status);
        Assert.Equal(5, context.BranchSum);
        Assert.Equal("merged:2", Assert.Single(context.Events));
        Assert.Equal(["branch-a", "branch-b"], forkResult.Children.Select(child => child.Name).ToArray());
    }

    [Fact]
    public async Task SupportsSkippingIndividualBranchesByCondition()
    {
        var context = new TestContext { SkipSecondBranch = true };
        var pipeline = PipelineBuilder<TestContext>
            .Create("branch-skip")
            .AddFork(
                "parallel-work",
                fork => fork
                    .AddBranch("branch-a", branch => branch.AddStep("a1", ctx => ctx.BranchSum += 1))
                    .AddBranch("branch-b", (ctx, _) => ValueTask.FromResult(!ctx.SkipSecondBranch), branch => branch.AddStep("b1", ctx => ctx.BranchSum += 100)),
                executionMode: BranchExecutionMode.Parallel)
            .Build();

        var result = await pipeline.ExecuteAsync(context);
        var forkResult = Assert.Single(result.Root.Children);

        Assert.Equal(ExecutionStatus.Success, result.Status);
        Assert.Equal(1, context.BranchSum);
        Assert.Equal(ExecutionStatus.Skipped, forkResult.Children.Single(child => child.Name == "branch-b").Status);
    }

    [Fact]
    public async Task FailFastStopsPipelineAfterFirstFailure()
    {
        var context = new TestContext();
        var pipeline = PipelineBuilder<TestContext>
            .Create("fail-fast")
            .Configure(new PipelineOptions { FailureMode = PipelineFailureMode.FailFast })
            .AddStep("step-1", ctx => ctx.Events.Enqueue("step-1"))
            .AddStep("step-2", _ => throw new InvalidOperationException("boom"))
            .AddStep("step-3", ctx => ctx.Events.Enqueue("step-3"))
            .Build();

        var result = await pipeline.ExecuteAsync(context);

        Assert.Equal(ExecutionStatus.Failed, result.Status);
        Assert.Equal(["step-1"], context.Events.ToArray());
        Assert.Single(result.FailedNodes);
        Assert.DoesNotContain(result.Root.Children, child => child.Name == "step-3");
    }

    [Fact]
    public async Task ContinueOnErrorKeepsRunningRemainingSteps()
    {
        var context = new TestContext();
        var pipeline = PipelineBuilder<TestContext>
            .Create("continue-on-error")
            .Configure(new PipelineOptions { FailureMode = PipelineFailureMode.ContinueOnError })
            .AddStep("step-1", ctx => ctx.Events.Enqueue("step-1"))
            .AddStep("step-2", _ => throw new InvalidOperationException("boom"))
            .AddStep("step-3", ctx => ctx.Events.Enqueue("step-3"))
            .Build();

        var result = await pipeline.ExecuteAsync(context);

        Assert.Equal(ExecutionStatus.Failed, result.Status);
        Assert.Equal(["step-1", "step-3"], context.Events.ToArray());
        Assert.Single(result.FailedNodes);
        Assert.Contains(result.Root.Children, child => child.Name == "step-3");
    }

    [Fact]
    public async Task PreservesDeterministicBranchOrderEvenWhenParallelCompletionDiffers()
    {
        var context = new TestContext();
        var pipeline = PipelineBuilder<TestContext>
            .Create("deterministic-order")
            .AddFork(
                "fork",
                fork => fork
                    .AddBranch("slow-branch", branch => branch.AddStep("slow", async (ctx, ct) =>
                    {
                        await Task.Delay(40, ct);
                        ctx.Events.Enqueue("slow");
                    }))
                    .AddBranch("fast-branch", branch => branch.AddStep("fast", async (ctx, ct) =>
                    {
                        await Task.Delay(1, ct);
                        ctx.Events.Enqueue("fast");
                    })),
                executionMode: BranchExecutionMode.Parallel)
            .Build();

        var result = await pipeline.ExecuteAsync(context);
        var forkResult = Assert.Single(result.Root.Children);

        Assert.Equal(["slow-branch", "fast-branch"], forkResult.Children.Select(child => child.Name).ToArray());
        Assert.Equal(ExecutionStatus.Success, result.Status);
    }

    [Fact]
    public async Task SupportsAsyncSteps()
    {
        var context = new TestContext();
        var pipeline = PipelineBuilder<TestContext>
            .Create("async")
            .AddStep("async-step", async (ctx, ct) =>
            {
                await Task.Delay(5, ct);
                ctx.Counter++;
            })
            .Build();

        var result = await pipeline.ExecuteAsync(context);

        Assert.Equal(ExecutionStatus.Success, result.Status);
        Assert.Equal(1, context.Counter);
    }

    [Fact]
    public async Task CancelsBeforeExecutionStarts()
    {
        var context = new TestContext();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var pipeline = PipelineBuilder<TestContext>
            .Create("cancelled-before-start")
            .AddStep("step", ctx => ctx.Events.Enqueue("never"))
            .Build();

        var result = await pipeline.ExecuteAsync(context, cts.Token);

        Assert.Equal(ExecutionStatus.Cancelled, result.Status);
        Assert.Empty(context.Events);
    }

    [Fact]
    public async Task CancelsWhileStepIsRunning()
    {
        var context = new TestContext();
        using var cts = new CancellationTokenSource();

        var pipeline = PipelineBuilder<TestContext>
            .Create("cancel-during-step")
            .AddStep("long-step", async (_, ct) => await Task.Delay(TimeSpan.FromSeconds(5), ct))
            .Build();

        cts.CancelAfter(50);
        var result = await pipeline.ExecuteAsync(context, cts.Token);

        Assert.Equal(ExecutionStatus.Cancelled, result.Status);
        Assert.Single(result.CancelledNodes);
    }

    [Fact]
    public async Task CancelsParallelBranchesConsistently()
    {
        var context = new TestContext();
        using var cts = new CancellationTokenSource();

        var pipeline = PipelineBuilder<TestContext>
            .Create("cancel-fork")
            .AddFork(
                "fork",
                fork => fork
                    .AddBranch("branch-a", branch => branch.AddStep("a", async (_, ct) => await Task.Delay(TimeSpan.FromSeconds(5), ct)))
                    .AddBranch("branch-b", branch => branch.AddStep("b", async (_, ct) => await Task.Delay(TimeSpan.FromSeconds(5), ct))),
                executionMode: BranchExecutionMode.Parallel)
            .Build();

        cts.CancelAfter(50);
        var result = await pipeline.ExecuteAsync(context, cts.Token);

        Assert.Equal(ExecutionStatus.Cancelled, result.Status);
        Assert.NotEmpty(result.CancelledNodes);
    }

    [Fact]
    public async Task BehaviorsCanEnrichResultMetadata()
    {
        var pipeline = PipelineBuilder<TestContext>
            .Create("metadata")
            .UseBehavior(new MetadataBehavior<TestContext>())
            .AddStep("step", _ => { })
            .Build();

        var result = await pipeline.ExecuteAsync(new TestContext());

        Assert.Equal("metadata", result.Root.Metadata["pipeline"]);
    }

    [Theory]
    [InlineData(PipelineFailureMode.FailFast, ExecutionStatus.Failed)]
    [InlineData(PipelineFailureMode.ContinueOnError, ExecutionStatus.Failed)]
    public async Task FailedStepMarksPipelineAsFailed(PipelineFailureMode failureMode, ExecutionStatus expectedStatus)
    {
        var pipeline = PipelineBuilder<TestContext>
            .Create("failure-status")
            .Configure(new PipelineOptions { FailureMode = failureMode })
            .AddStep("step", _ => throw new InvalidOperationException("boom"))
            .Build();

        var result = await pipeline.ExecuteAsync(new TestContext());

        Assert.Equal(expectedStatus, result.Status);
    }
}
