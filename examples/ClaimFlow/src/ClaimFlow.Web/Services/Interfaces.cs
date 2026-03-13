using ClaimFlow.Web.Domain;
using ClaimFlow.Web.Models;
using ClaimFlow.Web.Pipeline;
using TaskPipeline.Abstractions;

namespace ClaimFlow.Web.Services;

public interface IClaimValidationService
{
    Task<bool> ValidateAsync(ClaimRequest request, CancellationToken cancellationToken);
}

public interface IPolicyService
{
    Task<PolicySnapshot?> GetPolicyAsync(string policyNumber, CancellationToken cancellationToken);
    Task<CoverageCheckResult> CheckCoverageAsync(PolicySnapshot policy, ClaimRequest request, CancellationToken cancellationToken);
}

public interface IFraudService
{
    Task<FraudCheckResult> CheckAsync(ClaimRequest request, CancellationToken cancellationToken);
}

public interface IDocumentService
{
    Task<DocumentCompletenessResult> CheckCompletenessAsync(ClaimRequest request, CancellationToken cancellationToken);
    Task<DocumentVerificationResult> VerifyAsync(ClaimRequest request, CancellationToken cancellationToken);
}

public interface IDamageEstimator
{
    Task<DamageEstimateResult> EstimateAsync(ClaimRequest request, CancellationToken cancellationToken);
}

public interface IRepairQuoteService
{
    Task<RepairQuoteResult> GetBestQuoteAsync(ClaimRequest request, CancellationToken cancellationToken);
}

public interface IPaymentService
{
    Task<PaymentPrecheckResult> PrecheckAsync(ClaimRequest request, CancellationToken cancellationToken);
    Task ExecutePaymentAsync(Guid claimId, decimal amount, CancellationToken cancellationToken);
}

public interface INotificationService
{
    Task RequestMissingDocumentsAsync(Guid claimId, IReadOnlyList<string> missingDocuments, CancellationToken cancellationToken);
    Task NotifyClaimInProgressAsync(Guid claimId, CancellationToken cancellationToken);
    Task NotifyFinalDecisionAsync(Guid claimId, FinalDecision finalDecision, CancellationToken cancellationToken);
}

public interface IClaimRepository
{
    Task<ClaimCase?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<List<ClaimCase>> ListAsync(CancellationToken cancellationToken);
    Task AddAsync(ClaimCase claim, CancellationToken cancellationToken);
    Task SaveAsync(ClaimProcessingContext context, CancellationToken cancellationToken);
    Task AssignToAdjusterAsync(Guid claimId, CancellationToken cancellationToken);
    Task AddExecutionSnapshotAsync(ClaimExecutionSnapshot snapshot, CancellationToken cancellationToken);
}

public interface IClaimPipelineFactory
{
    IPipeline<ClaimProcessingContext> Create();
}
