using System.ComponentModel.DataAnnotations;

namespace ClaimFlow.Web.Domain;

public sealed class ClaimCase
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(128)]
    public string CustomerName { get; set; } = string.Empty;

    [MaxLength(128)]
    public string CustomerEmail { get; set; } = string.Empty;

    [MaxLength(32)]
    public string PolicyNumber { get; set; } = string.Empty;

    public ClaimType ClaimType { get; set; }
    public ClaimStatus Status { get; set; } = ClaimStatus.Draft;
    public FinalDecisionType FinalDecisionType { get; set; } = FinalDecisionType.None;

    [MaxLength(1024)]
    public string Description { get; set; } = string.Empty;

    public decimal EstimatedDamageAmount { get; set; }
    public decimal ApprovedPayoutAmount { get; set; }
    public bool HasPoliceReport { get; set; }
    public bool HasPhotos { get; set; }
    public bool RequiresTowTruck { get; set; }
    public bool InjuryInvolved { get; set; }
    public bool IsVipCustomer { get; set; }

    [MaxLength(2048)]
    public string MissingDocumentsJson { get; set; } = "[]";

    [MaxLength(2048)]
    public string WarningsJson { get; set; } = "[]";

    [MaxLength(4096)]
    public string FinalDecisionReason { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAtUtc { get; set; }

    public ICollection<ClaimExecutionSnapshot> Executions { get; set; } = new List<ClaimExecutionSnapshot>();
}
