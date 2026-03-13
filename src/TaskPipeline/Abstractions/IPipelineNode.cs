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
