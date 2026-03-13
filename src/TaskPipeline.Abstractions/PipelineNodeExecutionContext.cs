namespace TaskPipeline.Abstractions;

/// <summary>
/// Supplies metadata to behaviors running around a node.
/// </summary>
/// <typeparam name="TContext">Pipeline context type.</typeparam>
public sealed record PipelineNodeExecutionContext<TContext>(
    string PipelineName,
    string NodeName,
    NodeKind NodeKind,
    TContext Context);
