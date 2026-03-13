namespace TaskPipeline;

using TaskPipeline.Abstractions;

/// <summary>
/// Immutable branch description used by fork nodes.
/// </summary>
internal sealed record BranchDefinition<TContext>(
    IPipelineNode<TContext> Node,
    IPipelineCondition<TContext>? Condition);
