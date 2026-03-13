namespace Pipeline.Exceptions;

/// <summary>
/// Represents a failure while executing a pipeline step.
/// </summary>
public sealed class PipelineItemExecutionException : Exception
{
    public Guid? Id { get; }
    public string? StepName { get; }

    public PipelineItemExecutionException()
    {
    }

    public PipelineItemExecutionException(string message)
        : base(message)
    {
    }

    public PipelineItemExecutionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public PipelineItemExecutionException(Guid? id, string? stepName, string message, Exception innerException)
        : base(message, innerException)
    {
        Id = id;
        StepName = stepName;
    }

    public static PipelineItemExecutionException Wrap(Exception exception, Guid? id = null, string? stepName = null)
    {
        return exception as PipelineItemExecutionException
               ?? new PipelineItemExecutionException(id, stepName, exception.Message, exception);
    }
}
