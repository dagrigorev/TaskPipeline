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
    /// Returns failed terminal nodes in depth-first deterministic order.
    /// Aggregate containers such as the pipeline root or a wrapping sequence are excluded
    /// when they only mirror the failure of nested nodes.
    /// </summary>
    public IReadOnlyList<NodeExecutionResult> FailedNodes => FlattenTerminalStatuses(Root, ExecutionStatus.Failed)
        .ToArray();

    /// <summary>
    /// Returns cancelled terminal nodes in depth-first deterministic order.
    /// Aggregate containers such as the pipeline root or a wrapping sequence are excluded
    /// when they only mirror the cancellation of nested nodes.
    /// </summary>
    public IReadOnlyList<NodeExecutionResult> CancelledNodes => FlattenTerminalStatuses(Root, ExecutionStatus.Cancelled)
        .ToArray();

    private static IEnumerable<NodeExecutionResult> FlattenTerminalStatuses(NodeExecutionResult root, ExecutionStatus status)
    {
        foreach (var result in Flatten(root))
        {
            if (result.Status != status)
            {
                continue;
            }

            // Composite/container nodes often reflect the aggregated status of nested nodes.
            // The flattened accessors are intended to surface actionable nodes, so we skip
            // wrappers that already expose matching descendants.
            if (result.Children.Any(child => ContainsStatus(child, status)))
            {
                continue;
            }

            yield return result;
        }
    }

    private static bool ContainsStatus(NodeExecutionResult node, ExecutionStatus status)
    {
        if (node.Status == status)
        {
            return true;
        }

        foreach (var child in node.Children)
        {
            if (ContainsStatus(child, status))
            {
                return true;
            }
        }

        return false;
    }

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
