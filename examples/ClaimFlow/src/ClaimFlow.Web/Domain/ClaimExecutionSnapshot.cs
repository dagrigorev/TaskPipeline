namespace ClaimFlow.Web.Domain;

public sealed class ClaimExecutionSnapshot
{
    public int Id { get; set; }
    public Guid ClaimCaseId { get; set; }
    public ClaimCase ClaimCase { get; set; } = default!;

    public string PipelineName { get; set; } = string.Empty;
    public string PipelineStatus { get; set; } = string.Empty;
    public double DurationMs { get; set; }
    public string SummaryJson { get; set; } = "{}";
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
