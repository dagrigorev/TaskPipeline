namespace TaskPipeline.Abstractions;

/// <summary>
/// Controls global execution behavior for a pipeline instance.
/// </summary>
public sealed record PipelineOptions
{
    /// <summary>
    /// How the pipeline reacts to failures raised by steps or branches.
    /// </summary>
    public PipelineFailureMode FailureMode { get; init; } = PipelineFailureMode.FailFast;
}
