using ClaimFlow.Web.Domain;

namespace ClaimFlow.Web.Pipeline;

public sealed record FinalDecision(
    FinalDecisionType Type,
    string Reason,
    decimal? ApprovedAmount = null);

public sealed record PolicySnapshot(
    string PolicyNumber,
    bool IsActive,
    decimal CoverageLimit,
    bool CoversGlass,
    bool CoversTowTruck,
    bool SupportsAutoPayment);

public sealed record FraudCheckResult(double Score, bool RequiresManualReview, string Summary);
public sealed record CoverageCheckResult(bool IsCovered, string Reason);
public sealed record DocumentCompletenessResult(bool IsComplete, IReadOnlyList<string> MissingDocuments);
public sealed record DocumentVerificationResult(bool IsValid, IReadOnlyList<string> MissingDocuments);
public sealed record DamageEstimateResult(decimal Amount, string Summary);
public sealed record RepairQuoteResult(decimal Amount, string PartnerName);
public sealed record PaymentPrecheckResult(bool CanPayAutomatically, string Reason);
