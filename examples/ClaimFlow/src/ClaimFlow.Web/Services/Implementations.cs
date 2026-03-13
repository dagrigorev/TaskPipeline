using System.Text.Json;
using ClaimFlow.Web.Data;
using ClaimFlow.Web.Domain;
using ClaimFlow.Web.Models;
using ClaimFlow.Web.Pipeline;
using Microsoft.EntityFrameworkCore;

namespace ClaimFlow.Web.Services;

public sealed class ClaimValidationService : IClaimValidationService
{
    public Task<bool> ValidateAsync(ClaimRequest request, CancellationToken cancellationToken)
    {
        var valid = !string.IsNullOrWhiteSpace(request.CustomerName)
                    && !string.IsNullOrWhiteSpace(request.CustomerEmail)
                    && !string.IsNullOrWhiteSpace(request.PolicyNumber)
                    && request.Description.Length >= 10;

        return Task.FromResult(valid);
    }
}

public sealed class PolicyService(AppDbContext dbContext) : IPolicyService
{
    public async Task<PolicySnapshot?> GetPolicyAsync(string policyNumber, CancellationToken cancellationToken)
    {
        var record = await dbContext.Policies
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.PolicyNumber == policyNumber, cancellationToken);

        return record is null
            ? null
            : new PolicySnapshot(
                record.PolicyNumber,
                record.IsActive,
                record.CoverageLimit,
                record.CoversGlass,
                record.CoversTowTruck,
                record.SupportsAutoPayment);
    }

    public Task<CoverageCheckResult> CheckCoverageAsync(PolicySnapshot policy, ClaimRequest request, CancellationToken cancellationToken)
    {
        if (!policy.IsActive)
        {
            return Task.FromResult(new CoverageCheckResult(false, "Policy is inactive."));
        }

        if (request.RequiresTowTruck && !policy.CoversTowTruck)
        {
            return Task.FromResult(new CoverageCheckResult(false, "Tow truck service is not covered by the policy."));
        }

        if (request.Description.Contains("glass", StringComparison.OrdinalIgnoreCase) && !policy.CoversGlass)
        {
            return Task.FromResult(new CoverageCheckResult(false, "Glass damage is not covered by the policy."));
        }

        return Task.FromResult(new CoverageCheckResult(true, "Covered by policy."));
    }
}

public sealed class FraudService : IFraudService
{
    public Task<FraudCheckResult> CheckAsync(ClaimRequest request, CancellationToken cancellationToken)
    {
        var score = 0.12;

        if (!request.HasPoliceReport)
        {
            score += 0.18;
        }

        if (!request.HasPhotos)
        {
            score += 0.22;
        }

        if (request.InjuryInvolved)
        {
            score += 0.20;
        }

        if (request.Description.Contains("cash", StringComparison.OrdinalIgnoreCase))
        {
            score += 0.16;
        }

        if (request.IsVipCustomer)
        {
            score -= 0.04;
        }

        score = Math.Clamp(score, 0, 0.99);
        var requiresReview = score >= 0.55;
        var summary = requiresReview ? "High-risk signal pattern." : "No major fraud indicators.";

        return Task.FromResult(new FraudCheckResult(score, requiresReview, summary));
    }
}

public sealed class DocumentService : IDocumentService
{
    public Task<DocumentCompletenessResult> CheckCompletenessAsync(ClaimRequest request, CancellationToken cancellationToken)
    {
        var missing = new List<string>();

        if (!request.HasPhotos)
        {
            missing.Add("Damage photos");
        }

        if (!request.HasPoliceReport && request.InjuryInvolved)
        {
            missing.Add("Police report");
        }

        return Task.FromResult(new DocumentCompletenessResult(missing.Count == 0, missing));
    }

    public Task<DocumentVerificationResult> VerifyAsync(ClaimRequest request, CancellationToken cancellationToken)
    {
        var missing = new List<string>();

        if (!request.HasPhotos)
        {
            missing.Add("Damage photos");
        }

        var isValid = missing.Count == 0;
        return Task.FromResult(new DocumentVerificationResult(isValid, missing));
    }
}

public sealed class DamageEstimator : IDamageEstimator
{
    public Task<DamageEstimateResult> EstimateAsync(ClaimRequest request, CancellationToken cancellationToken)
    {
        decimal amount = request.ClaimType switch
        {
            ClaimType.Kasko => 1800m,
            ClaimType.Osago => 950m,
            _ => 750m
        };

        if (request.Description.Contains("bumper", StringComparison.OrdinalIgnoreCase))
        {
            amount += 450m;
        }

        if (request.Description.Contains("door", StringComparison.OrdinalIgnoreCase))
        {
            amount += 700m;
        }

        if (request.Description.Contains("engine", StringComparison.OrdinalIgnoreCase))
        {
            amount += 2900m;
        }

        if (request.InjuryInvolved)
        {
            amount += 600m;
        }

        return Task.FromResult(new DamageEstimateResult(amount, $"Estimated by rules engine: {amount:C}."));
    }
}

public sealed class RepairQuoteService : IRepairQuoteService
{
    public Task<RepairQuoteResult> GetBestQuoteAsync(ClaimRequest request, CancellationToken cancellationToken)
    {
        var partner = request.IsVipCustomer ? "Prime Auto Partner" : "City Repair Hub";
        var amount = request.ClaimType == ClaimType.Kasko ? 2100m : 1100m;
        return Task.FromResult(new RepairQuoteResult(amount, partner));
    }
}

public sealed class PaymentService : IPaymentService
{
    public Task<PaymentPrecheckResult> PrecheckAsync(ClaimRequest request, CancellationToken cancellationToken)
    {
        var canAutoPay = !request.InjuryInvolved;
        var reason = canAutoPay ? "Automatic payment is allowed." : "Human review required due to injury involvement.";
        return Task.FromResult(new PaymentPrecheckResult(canAutoPay, reason));
    }

    public Task ExecutePaymentAsync(Guid claimId, decimal amount, CancellationToken cancellationToken)
        => Task.CompletedTask;
}

public sealed class NotificationService : INotificationService
{
    public Task RequestMissingDocumentsAsync(Guid claimId, IReadOnlyList<string> missingDocuments, CancellationToken cancellationToken)
        => Task.CompletedTask;

    public Task NotifyClaimInProgressAsync(Guid claimId, CancellationToken cancellationToken)
        => Task.CompletedTask;

    public Task NotifyFinalDecisionAsync(Guid claimId, FinalDecision finalDecision, CancellationToken cancellationToken)
        => Task.CompletedTask;
}

public sealed class ClaimRepository(AppDbContext dbContext) : IClaimRepository
{
    public Task<ClaimCase?> GetAsync(Guid id, CancellationToken cancellationToken)
        => dbContext.Claims
            .Include(x => x.Executions.OrderByDescending(e => e.CreatedAtUtc))
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<List<ClaimCase>> ListAsync(CancellationToken cancellationToken)
        => dbContext.Claims
            .Include(x => x.Executions)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(ClaimCase claim, CancellationToken cancellationToken)
    {
        dbContext.Claims.Add(claim);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveAsync(ClaimProcessingContext context, CancellationToken cancellationToken)
    {
        var claim = await dbContext.Claims.FirstAsync(x => x.Id == context.ClaimId, cancellationToken);

        claim.Status = context.ClaimCase.Status;
        claim.EstimatedDamageAmount = context.ClaimCase.EstimatedDamageAmount;
        claim.ApprovedPayoutAmount = context.ClaimCase.ApprovedPayoutAmount;
        claim.FinalDecisionType = context.ClaimCase.FinalDecisionType;
        claim.FinalDecisionReason = context.ClaimCase.FinalDecisionReason;
        claim.MissingDocumentsJson = JsonSerializer.Serialize(context.MissingDocuments);
        claim.WarningsJson = JsonSerializer.Serialize(context.Warnings);
        claim.ProcessedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AssignToAdjusterAsync(Guid claimId, CancellationToken cancellationToken)
    {
        var claim = await dbContext.Claims.FirstAsync(x => x.Id == claimId, cancellationToken);
        claim.Status = ClaimStatus.ManualReview;
        claim.FinalDecisionReason = "Assigned to adjuster for manual assessment.";
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AddExecutionSnapshotAsync(ClaimExecutionSnapshot snapshot, CancellationToken cancellationToken)
    {
        dbContext.Executions.Add(snapshot);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
