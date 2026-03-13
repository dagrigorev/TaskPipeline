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
