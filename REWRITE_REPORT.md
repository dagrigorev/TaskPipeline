# TaskPipeline rewrite report

## Project tree

- `Directory.Build.props`
- `TaskPipeline.sln`
- `src/TaskPipeline/BranchDefinition.cs`
- `src/TaskPipeline/ConditionalNode.cs`
- `src/TaskPipeline/DelegateCondition.cs`
- `src/TaskPipeline/DelegateMergeStrategy.cs`
- `src/TaskPipeline/DelegateStep.cs`
- `src/TaskPipeline/ForkBuilder.cs`
- `src/TaskPipeline/ForkNode.cs`
- `src/TaskPipeline/NoOpMergeStrategy.cs`
- `src/TaskPipeline/Pipeline.cs`
- `src/TaskPipeline/PipelineBase.cs`
- `src/TaskPipeline/PipelineBuilder.cs`
- `src/TaskPipeline/SequenceNode.cs`
- `src/TaskPipeline/StepNode.cs`
- `src/TaskPipeline/TaskPipeline.csproj`
- `src/TaskPipeline/TimingBehavior.cs`
- `src/TaskPipeline.Abstractions/BranchExecutionMode.cs`
- `src/TaskPipeline.Abstractions/ExecutionStatus.cs`
- `src/TaskPipeline.Abstractions/IBranchMergeStrategy.cs`
- `src/TaskPipeline.Abstractions/IPipeline.cs`
- `src/TaskPipeline.Abstractions/IPipelineBehavior.cs`
- `src/TaskPipeline.Abstractions/IPipelineCondition.cs`
- `src/TaskPipeline.Abstractions/IPipelineNode.cs`
- `src/TaskPipeline.Abstractions/IPipelineStep.cs`
- `src/TaskPipeline.Abstractions/NodeExecutionResult.cs`
- `src/TaskPipeline.Abstractions/NodeKind.cs`
- `src/TaskPipeline.Abstractions/PipelineExecutionResult.cs`
- `src/TaskPipeline.Abstractions/PipelineFailureMode.cs`
- `src/TaskPipeline.Abstractions/PipelineNodeExecutionContext.cs`
- `src/TaskPipeline.Abstractions/PipelineOptions.cs`
- `src/TaskPipeline.Abstractions/TaskPipeline.Abstractions.csproj`
- `tests/TaskPipeline.Tests/PipelineExecutionTests.cs`
- `tests/TaskPipeline.Tests/TaskPipeline.Tests.csproj`
- `tests/TaskPipeline.Tests/TestContext.cs`

## Key files

### `src/TaskPipeline.Abstractions/IPipeline.cs`

```csharp
namespace TaskPipeline.Abstractions;

/// <summary>
/// Represents a strongly typed executable pipeline.
/// </summary>
/// <typeparam name="TContext">Pipeline context type.</typeparam>
public interface IPipeline<TContext>
{
    /// <summary>
    /// Gets the friendly pipeline name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Executes the pipeline for the provided context.
    /// </summary>
    /// <param name="context">Typed execution context.</param>
    /// <param name="cancellationToken">Cancellation token propagated through the entire graph.</param>
    ValueTask<PipelineExecutionResult> ExecuteAsync(TContext context, CancellationToken cancellationToken = default);
}

```

### `src/TaskPipeline.Abstractions/IPipelineStep.cs`

```csharp
namespace TaskPipeline.Abstractions;

/// <summary>
/// Represents a single strongly typed step.
/// </summary>
/// <typeparam name="TContext">Pipeline context type.</typeparam>
public interface IPipelineStep<in TContext>
{
    /// <summary>
    /// Gets the step name used in diagnostics and execution results.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Executes the step.
    /// </summary>
    ValueTask ExecuteAsync(TContext context, CancellationToken cancellationToken);
}

```

### `src/TaskPipeline.Abstractions/IPipelineCondition.cs`

```csharp
namespace TaskPipeline.Abstractions;

/// <summary>
/// Represents a strongly typed asynchronous condition.
/// </summary>
/// <typeparam name="TContext">Pipeline context type.</typeparam>
public interface IPipelineCondition<in TContext>
{
    /// <summary>
    /// Gets the condition name used in diagnostics.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Evaluates whether the associated node should execute.
    /// </summary>
    ValueTask<bool> CanExecuteAsync(TContext context, CancellationToken cancellationToken);
}

```

### `src/TaskPipeline.Abstractions/IPipelineNode.cs`

```csharp
namespace TaskPipeline.Abstractions;

/// <summary>
/// Represents an executable node in the pipeline graph.
/// </summary>
/// <typeparam name="TContext">Pipeline context type.</typeparam>
public interface IPipelineNode<TContext>
{
    /// <summary>
    /// Gets the node name used in execution reports.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the semantic node kind.
    /// </summary>
    NodeKind Kind { get; }

    /// <summary>
    /// Executes the node.
    /// </summary>
    ValueTask<NodeExecutionResult> ExecuteAsync(TContext context, CancellationToken cancellationToken = default);
}

```

### `src/TaskPipeline.Abstractions/NodeExecutionResult.cs`

```csharp
namespace TaskPipeline.Abstractions;

/// <summary>
/// Detailed result for a single executed node.
/// </summary>
public sealed record NodeExecutionResult
{
    /// <summary>
    /// Friendly node name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Node kind.
    /// </summary>
    public required NodeKind Kind { get; init; }

    /// <summary>
    /// Final execution status.
    /// </summary>
    public required ExecutionStatus Status { get; init; }

    /// <summary>
    /// Start timestamp in UTC.
    /// </summary>
    public required DateTimeOffset StartedAtUtc { get; init; }

    /// <summary>
    /// Total execution duration.
    /// </summary>
    public required TimeSpan Duration { get; init; }

    /// <summary>
    /// Exception raised during execution, when applicable.
    /// </summary>
    public Exception? Exception { get; init; }

    /// <summary>
    /// Arbitrary metadata that callers can use for diagnostics.
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();

    /// <summary>
    /// Child results. Branches and sequences populate this collection.
    /// </summary>
    public IReadOnlyList<NodeExecutionResult> Children { get; init; } = [];

    /// <summary>
    /// Creates a lightweight result instance.
    /// </summary>
    public static NodeExecutionResult Create(
        string name,
        NodeKind kind,
        ExecutionStatus status,
        DateTimeOffset startedAtUtc,
        TimeSpan duration,
        Exception? exception = null,
        IReadOnlyDictionary<string, string>? metadata = null,
        IReadOnlyList<NodeExecutionResult>? children = null)
    {
        return new NodeExecutionResult
        {
            Name = name,
            Kind = kind,
            Status = status,
            StartedAtUtc = startedAtUtc,
            Duration = duration,
            Exception = exception,
            Metadata = metadata ?? new Dictionary<string, string>(),
            Children = children ?? []
        };
    }
}

```

### `src/TaskPipeline.Abstractions/PipelineExecutionResult.cs`

```csharp
namespace TaskPipeline.Abstractions;

/// <summary>
/// Final pipeline result including the root node and flattened accessors.
/// </summary>
public sealed record PipelineExecutionResult
{
    /// <summary>
    /// Pipeline name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Overall pipeline status.
    /// </summary>
    public required ExecutionStatus Status { get; init; }

    /// <summary>
    /// Duration from the start of the pipeline until completion.
    /// </summary>
    public required TimeSpan Duration { get; init; }

    /// <summary>
    /// Root result node that contains the entire execution tree.
    /// </summary>
    public required NodeExecutionResult Root { get; init; }

    /// <summary>
    /// Returns all failed nodes in depth-first deterministic order.
    /// </summary>
    public IReadOnlyList<NodeExecutionResult> FailedNodes => Flatten(Root)
        .Where(result => result.Status == ExecutionStatus.Failed)
        .ToArray();

    /// <summary>
    /// Returns all cancelled nodes in depth-first deterministic order.
    /// </summary>
    public IReadOnlyList<NodeExecutionResult> CancelledNodes => Flatten(Root)
        .Where(result => result.Status == ExecutionStatus.Cancelled)
        .ToArray();

    private static IEnumerable<NodeExecutionResult> Flatten(NodeExecutionResult root)
    {
        yield return root;

        foreach (var child in root.Children)
        {
            foreach (var nested in Flatten(child))
            {
                yield return nested;
            }
        }
    }
}

```

### `src/TaskPipeline/Pipeline.cs`

```csharp
namespace TaskPipeline;

using System.Diagnostics;
using TaskPipeline.Abstractions;

/// <summary>
/// Production-oriented strongly typed pipeline implementation.
/// </summary>
public sealed class Pipeline<TContext> : IPipeline<TContext>
{
    private readonly IPipelineNode<TContext> _root;
    private readonly IReadOnlyList<IPipelineBehavior<TContext>> _behaviors;

    internal Pipeline(string name, IPipelineNode<TContext> root, PipelineOptions options, IReadOnlyList<IPipelineBehavior<TContext>> behaviors)
    {
        Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException("Pipeline name cannot be empty.", nameof(name)) : name;
        _root = root ?? throw new ArgumentNullException(nameof(root));
        Options = options ?? throw new ArgumentNullException(nameof(options));
        _behaviors = behaviors ?? throw new ArgumentNullException(nameof(behaviors));
    }

    public string Name { get; }

    public PipelineOptions Options { get; }

    public async ValueTask<PipelineExecutionResult> ExecuteAsync(TContext context, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var startedAt = DateTimeOffset.UtcNow;

        NodeExecutionResult root;
        try
        {
            root = await ExecuteWithBehaviorsAsync(_root, context, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException ex)
        {
            root = NodeExecutionResult.Create(Name, NodeKind.Pipeline, ExecutionStatus.Cancelled, startedAt, stopwatch.Elapsed, ex);
        }

        return new PipelineExecutionResult
        {
            Name = Name,
            Status = root.Status,
            Duration = stopwatch.Elapsed,
            Root = root.Kind == NodeKind.Pipeline
                ? root
                : root with { Name = Name, Kind = NodeKind.Pipeline }
        };
    }

    private ValueTask<NodeExecutionResult> ExecuteWithBehaviorsAsync(IPipelineNode<TContext> node, TContext context, CancellationToken cancellationToken)
    {
        var nodeContext = new PipelineNodeExecutionContext<TContext>(Name, node.Name, node.Kind, context);

        Func<ValueTask<NodeExecutionResult>> next = () => node.ExecuteAsync(context, cancellationToken);

        foreach (var behavior in _behaviors.Reverse())
        {
            var current = next;
            next = () => behavior.InvokeAsync(context, nodeContext, current, cancellationToken);
        }

        return next();
    }
}

```

### `src/TaskPipeline/PipelineBuilder.cs`

```csharp
namespace TaskPipeline;

using TaskPipeline.Abstractions;

/// <summary>
/// Fluent builder for strongly typed pipelines.
/// </summary>
public sealed class PipelineBuilder<TContext>
{
    private readonly string _name;
    private readonly List<IPipelineNode<TContext>> _nodes = [];
    private readonly List<IPipelineBehavior<TContext>> _behaviors = [];
    private PipelineOptions _options = new();

    private PipelineBuilder(string name)
    {
        _name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException("Pipeline name cannot be empty.", nameof(name)) : name;
    }

    /// <summary>
    /// Creates a new builder instance.
    /// </summary>
    public static PipelineBuilder<TContext> Create(string name) => new(name);

    /// <summary>
    /// Configures pipeline-wide execution options.
    /// </summary>
    public PipelineBuilder<TContext> Configure(PipelineOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        return this;
    }

    /// <summary>
    /// Adds a middleware-like behavior.
    /// </summary>
    public PipelineBuilder<TContext> UseBehavior(IPipelineBehavior<TContext> behavior)
    {
        _behaviors.Add(behavior ?? throw new ArgumentNullException(nameof(behavior)));
        return this;
    }

    /// <summary>
    /// Adds an existing step implementation.
    /// </summary>
    public PipelineBuilder<TContext> AddStep(IPipelineStep<TContext> step)
    {
        _nodes.Add(new StepNode<TContext>(step));
        return this;
    }

    /// <summary>
    /// Adds a delegate-backed step.
    /// </summary>
    public PipelineBuilder<TContext> AddStep(string name, Func<TContext, CancellationToken, ValueTask> action)
        => AddStep(new DelegateStep<TContext>(name, action));

    /// <summary>
    /// Adds a synchronous delegate-backed step.
    /// </summary>
    public PipelineBuilder<TContext> AddStep(string name, Action<TContext> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        return AddStep(name, (context, _) =>
        {
            action(context);
            return ValueTask.CompletedTask;
        });
    }

    /// <summary>
    /// Adds a conditional node that chooses one of two sub-pipelines.
    /// </summary>
    public PipelineBuilder<TContext> AddConditional(
        string name,
        Func<TContext, CancellationToken, ValueTask<bool>> condition,
        Action<PipelineBuilder<TContext>> whenTrue,
        Action<PipelineBuilder<TContext>>? whenFalse = null)
    {
        var trueBuilder = Create($"{name}:true").Configure(_options);
        whenTrue(trueBuilder);

        PipelineNodeBase<TContext>? falseNode = null;
        if (whenFalse is not null)
        {
            var falseBuilder = Create($"{name}:false").Configure(_options);
            whenFalse(falseBuilder);
            falseNode = falseBuilder.BuildRootNode();
        }

        var node = new ConditionalNode<TContext>(
            name,
            new DelegateCondition<TContext>($"{name}:condition", condition),
            trueBuilder.BuildRootNode(),
            falseNode);

        _nodes.Add(node);
        return this;
    }

    /// <summary>
    /// Adds a fork node that executes multiple named branches.
    /// </summary>
    public PipelineBuilder<TContext> AddFork(
        string name,
        Action<ForkBuilder<TContext>> configure,
        BranchExecutionMode executionMode = BranchExecutionMode.Parallel,
        IBranchMergeStrategy<TContext>? mergeStrategy = null)
    {
        var forkBuilder = new ForkBuilder<TContext>(_options);
        configure(forkBuilder);

        var node = new ForkNode<TContext>(
            name,
            forkBuilder.Build(),
            executionMode,
            mergeStrategy ?? NoOpMergeStrategy<TContext>.Instance,
            _options);

        _nodes.Add(node);
        return this;
    }

    /// <summary>
    /// Builds the pipeline.
    /// </summary>
    public Pipeline<TContext> Build()
        => new(_name, BuildRootNode(), _options, _behaviors.AsReadOnly());

    internal SequenceNode<TContext> BuildRootNode()
        => new(_name, _nodes.AsReadOnly(), _options);
}

```

### `src/TaskPipeline/ForkBuilder.cs`

```csharp
namespace TaskPipeline;

using TaskPipeline.Abstractions;

/// <summary>
/// Builds named branches for a fork node.
/// </summary>
public sealed class ForkBuilder<TContext>
{
    private readonly PipelineOptions _options;
    private readonly List<BranchDefinition<TContext>> _branches = [];

    internal ForkBuilder(PipelineOptions options)
    {
        _options = options;
    }

    /// <summary>
    /// Adds a branch that always executes.
    /// </summary>
    public ForkBuilder<TContext> AddBranch(string name, Action<PipelineBuilder<TContext>> configure)
        => AddBranch(name, null, configure);

    /// <summary>
    /// Adds a branch that can be skipped by a typed condition.
    /// </summary>
    public ForkBuilder<TContext> AddBranch(
        string name,
        Func<TContext, CancellationToken, ValueTask<bool>>? condition,
        Action<PipelineBuilder<TContext>> configure)
    {
        var builder = PipelineBuilder<TContext>.Create(name).Configure(_options);
        configure(builder);

        var branchCondition = condition is null
            ? null
            : new DelegateCondition<TContext>($"{name}:condition", condition);

        _branches.Add(new BranchDefinition<TContext>(builder.BuildRootNode(), branchCondition));
        return this;
    }

    internal IReadOnlyList<BranchDefinition<TContext>> Build() => _branches.AsReadOnly();
}

```

### `src/TaskPipeline/SequenceNode.cs`

```csharp
namespace TaskPipeline;

using TaskPipeline.Abstractions;

/// <summary>
/// Executes child nodes in declaration order.
/// </summary>
internal sealed class SequenceNode<TContext>(string name, IReadOnlyList<IPipelineNode<TContext>> children, PipelineOptions options) : PipelineNodeBase<TContext>(name, NodeKind.Sequence)
{
    private readonly IReadOnlyList<IPipelineNode<TContext>> _children = children ?? throw new ArgumentNullException(nameof(children));
    private readonly PipelineOptions _options = options ?? throw new ArgumentNullException(nameof(options));

    protected override ValueTask<NodeExecutionResult> ExecuteCoreAsync(TContext context, CancellationToken cancellationToken)
    {
        return ExecuteWithResultAsync(
            Name,
            Kind,
            async ct =>
            {
                var results = new List<NodeExecutionResult>(_children.Count);

                foreach (var child in _children)
                {
                    ct.ThrowIfCancellationRequested();
                    var childResult = await child.ExecuteAsync(context, ct).ConfigureAwait(false);
                    results.Add(childResult);

                    if (childResult.Status == ExecutionStatus.Cancelled)
                    {
                        break;
                    }

                    if (_options.FailureMode == PipelineFailureMode.FailFast && childResult.Status == ExecutionStatus.Failed)
                    {
                        break;
                    }
                }

                return (ResolveAggregateStatus(results), null, (IReadOnlyList<NodeExecutionResult>)results, (IReadOnlyDictionary<string, string>?)null);
            },
            cancellationToken);
    }

    private static ExecutionStatus ResolveAggregateStatus(IReadOnlyList<NodeExecutionResult> results)
    {
        if (results.Count == 0)
        {
            return ExecutionStatus.Skipped;
        }

        if (results.Any(result => result.Status == ExecutionStatus.Cancelled))
        {
            return ExecutionStatus.Cancelled;
        }

        if (results.Any(result => result.Status == ExecutionStatus.Failed))
        {
            return ExecutionStatus.Failed;
        }

        if (results.All(result => result.Status == ExecutionStatus.Skipped))
        {
            return ExecutionStatus.Skipped;
        }

        return ExecutionStatus.Success;
    }
}

```

### `src/TaskPipeline/ConditionalNode.cs`

```csharp
namespace TaskPipeline;

using TaskPipeline.Abstractions;

/// <summary>
/// Chooses one branch based on a typed asynchronous predicate.
/// </summary>
internal sealed class ConditionalNode<TContext>(
    string name,
    IPipelineCondition<TContext> condition,
    IPipelineNode<TContext> whenTrue,
    IPipelineNode<TContext>? whenFalse) : PipelineNodeBase<TContext>(name, NodeKind.Conditional)
{
    private readonly IPipelineCondition<TContext> _condition = condition ?? throw new ArgumentNullException(nameof(condition));
    private readonly IPipelineNode<TContext> _whenTrue = whenTrue ?? throw new ArgumentNullException(nameof(whenTrue));
    private readonly IPipelineNode<TContext>? _whenFalse = whenFalse;

    protected override ValueTask<NodeExecutionResult> ExecuteCoreAsync(TContext context, CancellationToken cancellationToken)
    {
        return ExecuteWithResultAsync(
            Name,
            Kind,
            async ct =>
            {
                var canExecute = await _condition.CanExecuteAsync(context, ct).ConfigureAwait(false);
                var children = new List<NodeExecutionResult>(1);

                if (!canExecute)
                {
                    if (_whenFalse is null)
                    {
                        return (ExecutionStatus.Skipped, null, (IReadOnlyList<NodeExecutionResult>)children, new Dictionary<string, string>
                        {
                            ["condition"] = _condition.Name,
                            ["selected"] = "none"
                        });
                    }

                    var falseResult = await _whenFalse.ExecuteAsync(context, ct).ConfigureAwait(false);
                    children.Add(falseResult);

                    return (falseResult.Status, falseResult.Exception, (IReadOnlyList<NodeExecutionResult>)children, new Dictionary<string, string>
                    {
                        ["condition"] = _condition.Name,
                        ["selected"] = "false"
                    });
                }

                var trueResult = await _whenTrue.ExecuteAsync(context, ct).ConfigureAwait(false);
                children.Add(trueResult);

                return (trueResult.Status, trueResult.Exception, (IReadOnlyList<NodeExecutionResult>)children, new Dictionary<string, string>
                {
                    ["condition"] = _condition.Name,
                    ["selected"] = "true"
                });
            },
            cancellationToken);
    }
}

```

### `src/TaskPipeline/ForkNode.cs`

```csharp
namespace TaskPipeline;

using TaskPipeline.Abstractions;

/// <summary>
/// Executes multiple child branches either sequentially or in parallel and then optionally merges their outputs.
/// </summary>
internal sealed class ForkNode<TContext>(
    string name,
    IReadOnlyList<BranchDefinition<TContext>> branches,
    BranchExecutionMode executionMode,
    IBranchMergeStrategy<TContext> mergeStrategy,
    PipelineOptions options) : PipelineNodeBase<TContext>(name, NodeKind.Fork)
{
    private readonly IReadOnlyList<BranchDefinition<TContext>> _branches = branches ?? throw new ArgumentNullException(nameof(branches));
    private readonly BranchExecutionMode _executionMode = executionMode;
    private readonly IBranchMergeStrategy<TContext> _mergeStrategy = mergeStrategy ?? throw new ArgumentNullException(nameof(mergeStrategy));
    private readonly PipelineOptions _options = options ?? throw new ArgumentNullException(nameof(options));

    protected override ValueTask<NodeExecutionResult> ExecuteCoreAsync(TContext context, CancellationToken cancellationToken)
    {
        return ExecuteWithResultAsync(
            Name,
            Kind,
            async ct =>
            {
                IReadOnlyList<NodeExecutionResult> branchResults = _executionMode == BranchExecutionMode.Parallel
                    ? await ExecuteParallelAsync(context, ct).ConfigureAwait(false)
                    : await ExecuteSequentialAsync(context, ct).ConfigureAwait(false);

                var status = ResolveAggregateStatus(branchResults);

                if (status is ExecutionStatus.Success or ExecutionStatus.Skipped)
                {
                    await _mergeStrategy.MergeAsync(context, branchResults, ct).ConfigureAwait(false);
                }

                return (status, null, branchResults, new Dictionary<string, string>
                {
                    ["mode"] = _executionMode.ToString(),
                    ["merge"] = _mergeStrategy.Name
                });
            },
            cancellationToken);
    }

    private async ValueTask<IReadOnlyList<NodeExecutionResult>> ExecuteSequentialAsync(TContext context, CancellationToken cancellationToken)
    {
        var results = new List<NodeExecutionResult>(_branches.Count);

        foreach (var branch in _branches)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await ExecuteBranchAsync(branch, context, cancellationToken).ConfigureAwait(false);
            results.Add(result);

            if (_options.FailureMode == PipelineFailureMode.FailFast && result.Status == ExecutionStatus.Failed)
            {
                break;
            }

            if (result.Status == ExecutionStatus.Cancelled)
            {
                break;
            }
        }

        return results;
    }

    private async ValueTask<IReadOnlyList<NodeExecutionResult>> ExecuteParallelAsync(TContext context, CancellationToken cancellationToken)
    {
        var tasks = _branches
            .Select((branch, index) => ExecuteBranchWithIndexAsync(branch, index, context, cancellationToken))
            .ToArray();

        var completed = await Task.WhenAll(tasks).ConfigureAwait(false);

        // Results are ordered by declaration, not by completion time.
        return completed
            .OrderBy(result => result.Index)
            .Select(result => result.Execution)
            .ToArray();
    }

    private async ValueTask<(int Index, NodeExecutionResult Execution)> ExecuteBranchWithIndexAsync(
        BranchDefinition<TContext> branch,
        int index,
        TContext context,
        CancellationToken cancellationToken)
    {
        var result = await ExecuteBranchAsync(branch, context, cancellationToken).ConfigureAwait(false);
        return (index, result);
    }

    private static async ValueTask<NodeExecutionResult> ExecuteBranchAsync(BranchDefinition<TContext> branch, TContext context, CancellationToken cancellationToken)
    {
        if (branch.Condition is not null)
        {
            var shouldRun = await branch.Condition.CanExecuteAsync(context, cancellationToken).ConfigureAwait(false);
            if (!shouldRun)
            {
                return NodeExecutionResult.Create(
                    branch.Node.Name,
                    NodeKind.Branch,
                    ExecutionStatus.Skipped,
                    DateTimeOffset.UtcNow,
                    TimeSpan.Zero,
                    metadata: new Dictionary<string, string> { ["condition"] = branch.Condition.Name });
            }
        }

        var execution = await branch.Node.ExecuteAsync(context, cancellationToken).ConfigureAwait(false);
        return execution with { Kind = NodeKind.Branch };
    }

    private static ExecutionStatus ResolveAggregateStatus(IReadOnlyList<NodeExecutionResult> results)
    {
        if (results.Count == 0)
        {
            return ExecutionStatus.Skipped;
        }

        if (results.Any(result => result.Status == ExecutionStatus.Cancelled))
        {
            return ExecutionStatus.Cancelled;
        }

        if (results.Any(result => result.Status == ExecutionStatus.Failed))
        {
            return ExecutionStatus.Failed;
        }

        if (results.All(result => result.Status == ExecutionStatus.Skipped))
        {
            return ExecutionStatus.Skipped;
        }

        return ExecutionStatus.Success;
    }
}

```

### `tests/TaskPipeline.Tests/PipelineExecutionTests.cs`

```csharp
namespace TaskPipeline.Tests;

using TaskPipeline.Abstractions;
using Xunit;

public sealed class PipelineExecutionTests
{
    [Fact]
    public async Task Executes_steps_sequentially_in_declaration_order()
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
    public async Task Executes_true_conditional_branch_and_skips_false_branch()
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
    public async Task Skips_conditional_node_when_condition_is_false_and_no_else_branch_exists()
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
    public async Task Aggregates_parallel_branch_results_and_runs_merge_strategy()
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
    public async Task Supports_skipping_individual_branches_by_condition()
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
    public async Task Fail_fast_stops_pipeline_after_first_failure()
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
    public async Task Continue_on_error_keeps_running_remaining_steps()
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
    public async Task Preserves_deterministic_branch_order_even_when_parallel_completion_differs()
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
    public async Task Supports_async_steps()
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
    public async Task Cancels_before_execution_starts()
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
    public async Task Cancels_while_step_is_running()
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
    public async Task Cancels_parallel_branches_consistently()
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
    public async Task Behaviors_can_enrich_result_metadata()
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
    public async Task Failed_step_marks_pipeline_as_failed(PipelineFailureMode failureMode, ExecutionStatus expectedStatus)
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

```
