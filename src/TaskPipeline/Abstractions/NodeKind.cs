namespace TaskPipeline.Abstractions;

/// <summary>
/// Describes the semantic role of a pipeline node.
/// </summary>
public enum NodeKind
{
    Pipeline,
    Sequence,
    Step,
    Conditional,
    Fork,
    Branch
}
