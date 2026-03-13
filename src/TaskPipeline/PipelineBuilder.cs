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
