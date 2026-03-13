using ClaimFlow.Web.Domain;
using ClaimFlow.Web.Pipeline;

namespace ClaimFlow.Web.Models;

public sealed class ClaimProcessingContext
{
    public Guid ClaimId { get; init; }
    public ClaimRequest Request { get; init; } = default!;
    public ClaimCase ClaimCase { get; init; } = default!;

    public bool IsValid { get; set; }
    public bool IsComplete { get; set; }
    public bool RequiresManualReview { get; set; }
    public bool IsHighRisk { get; set; }
    public bool IsEligibleForAutoApproval { get; set; }

    public PolicySnapshot? Policy { get; set; }
    public FraudCheckResult? FraudCheck { get; set; }
    public CoverageCheckResult? CoverageCheck { get; set; }
    public DocumentVerificationResult? DocumentVerification { get; set; }
    public DamageEstimateResult? DamageEstimate { get; set; }
    public RepairQuoteResult? RepairQuote { get; set; }
    public PaymentPrecheckResult? PaymentPrecheck { get; set; }

    public List<string> MissingDocuments { get; } = [];
    public List<string> Notifications { get; } = [];
    public List<string> AuditTrail { get; } = [];
    public List<string> Warnings { get; } = [];

    public FinalDecision? FinalDecision { get; set; }

    public void AddAudit(string message) => AuditTrail.Add(message);
    public void AddWarning(string message) => Warnings.Add(message);
}
