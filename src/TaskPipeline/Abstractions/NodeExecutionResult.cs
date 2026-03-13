namespace TaskPipeline.Abstractions;

/// <summary>
/// Detailed result for a single executed node.
/// </summary>
public sealed record NodeExecutionResult
{
    /// <summary>
    /// Friendly node name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Node kind.
    /// </summary>
    public required NodeKind Kind { get; init; }

    /// <summary>
    /// Final execution status.
    /// </summary>
    public required ExecutionStatus Status { get; init; }

    /// <summary>
    /// Start timestamp in UTC.
    /// </summary>
    public required DateTimeOffset StartedAtUtc { get; init; }

    /// <summary>
    /// Total execution duration.
    /// </summary>
    public required TimeSpan Duration { get; init; }

    /// <summary>
    /// Exception raised during execution, when applicable.
    /// </summary>
    public Exception? Exception { get; init; }

    /// <summary>
    /// Arbitrary metadata that callers can use for diagnostics.
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();

    /// <summary>
    /// Child results. Branches and sequences populate this collection.
    /// </summary>
    public IReadOnlyList<NodeExecutionResult> Children { get; init; } = [];

    /// <summary>
    /// Creates a lightweight result instance.
    /// </summary>
    public static NodeExecutionResult Create(
        string name,
        NodeKind kind,
        ExecutionStatus status,
        DateTimeOffset startedAtUtc,
        TimeSpan duration,
        Exception? exception = null,
        IReadOnlyDictionary<string, string>? metadata = null,
        IReadOnlyList<NodeExecutionResult>? children = null)
    {
        return new NodeExecutionResult
        {
            Name = name,
            Kind = kind,
            Status = status,
            StartedAtUtc = startedAtUtc,
            Duration = duration,
            Exception = exception,
            Metadata = metadata ?? new Dictionary<string, string>(),
            Children = children ?? []
        };
    }
}
