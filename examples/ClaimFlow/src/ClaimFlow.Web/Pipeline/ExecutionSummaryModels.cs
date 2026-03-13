namespace ClaimFlow.Web.Pipeline;

public sealed class ExecutionSummary
{
    public string Status { get; set; } = string.Empty;
    public double DurationMs { get; set; }
    public List<ExecutionNodeSummary> FailedNodes { get; set; } = [];
    public List<ExecutionNodeSummary> CancelledNodes { get; set; } = [];
    public ExecutionNodeSummary? Root { get; set; }
}

public sealed class ExecutionNodeSummary
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public double DurationMs { get; set; }
    public string? Exception { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = [];
    public List<ExecutionNodeSummary> Children { get; set; } = [];
}
