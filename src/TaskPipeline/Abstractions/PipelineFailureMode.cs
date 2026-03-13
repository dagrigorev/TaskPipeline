namespace TaskPipeline.Abstractions;

/// <summary>
/// Defines how the pipeline reacts to failures.
/// </summary>
public enum PipelineFailureMode
{
    FailFast,
    ContinueOnError
}
