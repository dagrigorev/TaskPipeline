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

        var completed = await Task.WhenAll(tasks.Select(t => t.AsTask())).ConfigureAwait(false);

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
