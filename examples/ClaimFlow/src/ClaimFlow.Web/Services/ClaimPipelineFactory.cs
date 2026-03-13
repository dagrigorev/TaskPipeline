using ClaimFlow.Web.Domain;
using ClaimFlow.Web.Models;
using ClaimFlow.Web.Pipeline;
using TaskPipeline;
using TaskPipeline.Abstractions;

namespace ClaimFlow.Web.Services;

public sealed class ClaimPipelineFactory(
    IClaimValidationService validationService,
    IPolicyService policyService,
    IFraudService fraudService,
    IDocumentService documentService,
    IDamageEstimator damageEstimator,
    IRepairQuoteService repairQuoteService,
    IPaymentService paymentService,
    INotificationService notificationService,
    IClaimRepository claimRepository) : IClaimPipelineFactory
{
    public IPipeline<ClaimProcessingContext> Create()
    {
        return PipelineBuilder<ClaimProcessingContext>
            .Create("claim-processing")
            .Configure(new PipelineOptions
            {
                FailureMode = PipelineFailureMode.ContinueOnError
            })
            .AddStep("mark-validating", ctx =>
            {
                ctx.ClaimCase.Status = ClaimStatus.Validating;
                ctx.AddAudit("Claim moved to validation stage.");
            })
            .AddStep("validate-request", async (ctx, ct) =>
            {
                ctx.IsValid = await validationService.ValidateAsync(ctx.Request, ct);
                if (!ctx.IsValid)
                {
                    throw new InvalidOperationException("Claim request is invalid.");
                }

                ctx.AddAudit("Claim request validated.");
            })
            .AddStep("attach-policy", async (ctx, ct) =>
            {
                ctx.Policy = await policyService.GetPolicyAsync(ctx.Request.PolicyNumber, ct)
                    ?? throw new InvalidOperationException("Policy not found.");

                ctx.AddAudit($"Policy attached: {ctx.Policy.PolicyNumber}");
            })
            .AddConditional(
                "check-policy-found",
                (ctx, _) => ValueTask.FromResult(ctx.Policy is not null),
                whenTrue: branch => branch
                    .AddConditional(
                        "check-policy-active",
                        (ctx, _) => ValueTask.FromResult(ctx.Policy?.IsActive == true),
                        whenTrue: b => b.AddStep("policy-active-log", ctx =>
                        {
                            ctx.AddAudit("Policy is active.");
                        }),
                        whenFalse: b => b.AddStep("reject-inactive-policy", ctx =>
                        {
                            ctx.FinalDecision = new FinalDecision(
                                FinalDecisionType.Reject,
                                "Policy is inactive.");

                            ctx.ClaimCase.Status = ClaimStatus.Rejected;
                            ctx.ClaimCase.FinalDecisionType = FinalDecisionType.Reject;
                            ctx.ClaimCase.FinalDecisionReason = "Policy is inactive";
                            ctx.AddAudit("Claim rejected because policy is inactive.");
                        })),
                whenFalse: branch => branch.AddStep("reject-policy-not-found", ctx =>
                {
                    ctx.FinalDecision = new FinalDecision(
                        FinalDecisionType.Reject,
                        "Policy not found.");

                    ctx.ClaimCase.Status = ClaimStatus.Rejected;
                    ctx.ClaimCase.FinalDecisionType = FinalDecisionType.Reject;
                    ctx.ClaimCase.FinalDecisionReason = "Policy not found";
                    ctx.AddAudit("Claim rejected because policy was not found.");
                }))
            .AddConditional(
                "check-policy-active",
                (ctx, _) => ValueTask.FromResult(ctx.Policy?.IsActive == true),
                whenTrue: branch => branch.AddStep("policy-active-log", ctx => ctx.AddAudit("Policy is active.")),
                whenFalse: branch => branch.AddStep("reject-inactive-policy", ctx =>
                {
                    ctx.FinalDecision = new FinalDecision(FinalDecisionType.Reject, "Policy is inactive.");
                    ctx.ClaimCase.Status = ClaimStatus.Rejected;
                    ctx.ClaimCase.FinalDecisionType = FinalDecisionType.Reject;
                    ctx.ClaimCase.FinalDecisionReason = "Policy is inactive.";
                    ctx.AddAudit("Claim rejected because policy is inactive.");
                }))
            .AddConditional(
                "check-completeness",
                async (ctx, ct) =>
                {
                    var result = await documentService.CheckCompletenessAsync(ctx.Request, ct);
                    ctx.IsComplete = result.IsComplete;
                    ctx.MissingDocuments.Clear();
                    ctx.MissingDocuments.AddRange(result.MissingDocuments);
                    return ctx.IsComplete;
                },
                whenTrue: branch => branch.AddStep("mark-assessing", ctx =>
                {
                    ctx.ClaimCase.Status = ClaimStatus.Assessing;
                    ctx.AddAudit("Claim package is complete.");
                }),
                whenFalse: branch => branch.AddStep("request-missing-documents", async (ctx, ct) =>
                {
                    ctx.FinalDecision = new FinalDecision(FinalDecisionType.RequestDocuments, "Missing required documents.");
                    ctx.ClaimCase.Status = ClaimStatus.WaitingForDocuments;
                    ctx.ClaimCase.FinalDecisionType = FinalDecisionType.RequestDocuments;
                    ctx.ClaimCase.FinalDecisionReason = "Missing required documents.";

                    await notificationService.RequestMissingDocumentsAsync(ctx.ClaimId, ctx.MissingDocuments, ct);
                    ctx.AddAudit("Missing documents requested from customer.");
                }))
            .AddConditional(
                "continue-only-when-complete",
                (ctx, _) => ValueTask.FromResult(ctx.IsComplete),
                whenTrue: branch => branch
                    .AddFork(
                        "parallel-assessment",
                        fork => fork
                            .AddBranch("fraud-check", pipeline => pipeline.AddStep("run-fraud-check", async (ctx, ct) =>
                            {
                                ctx.FraudCheck = await fraudService.CheckAsync(ctx.Request, ct);
                                ctx.IsHighRisk = ctx.FraudCheck.RequiresManualReview;
                                ctx.AddAudit($"Fraud score: {ctx.FraudCheck.Score:0.00}");
                            }))
                            .AddBranch("coverage-check", pipeline => pipeline.AddStep("run-coverage-check", async (ctx, ct) =>
                            {
                                ctx.CoverageCheck = await policyService.CheckCoverageAsync(ctx.Policy!, ctx.Request, ct);
                                ctx.AddAudit($"Coverage result: {ctx.CoverageCheck.IsCovered}");
                            }))
                            .AddBranch("document-verification", pipeline => pipeline.AddStep("verify-documents", async (ctx, ct) =>
                            {
                                ctx.DocumentVerification = await documentService.VerifyAsync(ctx.Request, ct);
                                if (!ctx.DocumentVerification.IsValid)
                                {
                                    foreach (var item in ctx.DocumentVerification.MissingDocuments)
                                    {
                                        if (!ctx.MissingDocuments.Contains(item, StringComparer.OrdinalIgnoreCase))
                                        {
                                            ctx.MissingDocuments.Add(item);
                                        }
                                    }
                                }

                                ctx.AddAudit("Documents verified.");
                            }))
                            .AddBranch("damage-estimation", pipeline => pipeline.AddStep("estimate-damage", async (ctx, ct) =>
                            {
                                ctx.DamageEstimate = await damageEstimator.EstimateAsync(ctx.Request, ct);
                                ctx.ClaimCase.EstimatedDamageAmount = ctx.DamageEstimate.Amount;
                                ctx.AddAudit($"Estimated damage amount: {ctx.ClaimCase.EstimatedDamageAmount:0.00}");
                            }))
                            .AddBranch("repair-quote", pipeline => pipeline.AddStep("get-repair-quote", async (ctx, ct) =>
                            {
                                ctx.RepairQuote = await repairQuoteService.GetBestQuoteAsync(ctx.Request, ct);
                                ctx.AddAudit($"Repair quote received: {ctx.RepairQuote.Amount:0.00}");
                            }))
                            .AddBranch("payment-precheck", pipeline => pipeline.AddStep("run-payment-precheck", async (ctx, ct) =>
                            {
                                ctx.PaymentPrecheck = await paymentService.PrecheckAsync(ctx.Request, ct);
                                ctx.AddAudit($"Payment precheck: {ctx.PaymentPrecheck.CanPayAutomatically}");
                            }))
                            .AddBranch("customer-notification", pipeline => pipeline.AddStep("notify-processing-started", async (ctx, ct) =>
                            {
                                await notificationService.NotifyClaimInProgressAsync(ctx.ClaimId, ct);
                                ctx.Notifications.Add("Processing started notification sent.");
                            })),
                        executionMode: BranchExecutionMode.Parallel,
                        mergeStrategy: new DelegateMergeStrategy<ClaimProcessingContext>(
                            "build-final-decision",
                            async (ctx, _, ct) =>
                            {
                                if (ctx.CoverageCheck is { IsCovered: false })
                                {
                                    ctx.FinalDecision = new FinalDecision(FinalDecisionType.Reject, ctx.CoverageCheck.Reason);
                                    ctx.ClaimCase.Status = ClaimStatus.Rejected;
                                    ctx.ClaimCase.FinalDecisionType = FinalDecisionType.Reject;
                                    ctx.ClaimCase.FinalDecisionReason = ctx.CoverageCheck.Reason;
                                    return;
                                }

                                if (ctx.IsHighRisk || ctx.Request.InjuryInvolved)
                                {
                                    ctx.RequiresManualReview = true;
                                    ctx.FinalDecision = new FinalDecision(FinalDecisionType.ManualReview, "High-risk or injury case requires manual review.");
                                    ctx.ClaimCase.Status = ClaimStatus.ManualReview;
                                    ctx.ClaimCase.FinalDecisionType = FinalDecisionType.ManualReview;
                                    ctx.ClaimCase.FinalDecisionReason = "High-risk or injury case requires manual review.";
                                    return;
                                }

                                if (ctx.DocumentVerification is { IsValid: false })
                                {
                                    ctx.FinalDecision = new FinalDecision(FinalDecisionType.RequestDocuments, "Document verification failed.");
                                    ctx.ClaimCase.Status = ClaimStatus.WaitingForDocuments;
                                    ctx.ClaimCase.FinalDecisionType = FinalDecisionType.RequestDocuments;
                                    ctx.ClaimCase.FinalDecisionReason = "Document verification failed.";
                                    return;
                                }

                                var amount = ctx.RepairQuote?.Amount ?? ctx.DamageEstimate?.Amount ?? 0m;
                                ctx.ClaimCase.ApprovedPayoutAmount = amount;

                                var canAutoPay = ctx.PaymentPrecheck?.CanPayAutomatically == true
                                                 && amount > 0
                                                 && amount <= ctx.Policy!.CoverageLimit
                                                 && ctx.Policy.SupportsAutoPayment;

                                if (canAutoPay)
                                {
                                    ctx.IsEligibleForAutoApproval = true;
                                    ctx.FinalDecision = new FinalDecision(FinalDecisionType.Approve, "Automatically approved.", amount);
                                    ctx.ClaimCase.Status = ClaimStatus.Approved;
                                    ctx.ClaimCase.FinalDecisionType = FinalDecisionType.Approve;
                                    ctx.ClaimCase.FinalDecisionReason = "Automatically approved.";
                                    return;
                                }

                                ctx.RequiresManualReview = true;
                                ctx.FinalDecision = new FinalDecision(FinalDecisionType.ManualReview, "Requires adjuster review.");
                                ctx.ClaimCase.Status = ClaimStatus.ManualReview;
                                ctx.ClaimCase.FinalDecisionType = FinalDecisionType.ManualReview;
                                ctx.ClaimCase.FinalDecisionReason = "Requires adjuster review.";
                                await Task.CompletedTask;
                            }))
                    .AddConditional(
                        "auto-pay-or-manual",
                        (ctx, _) => ValueTask.FromResult(ctx.IsEligibleForAutoApproval),
                        whenTrue: pipeline => pipeline.AddStep("execute-payment", async (ctx, ct) =>
                        {
                            await paymentService.ExecutePaymentAsync(ctx.ClaimId, ctx.ClaimCase.ApprovedPayoutAmount, ct);
                            ctx.ClaimCase.Status = ClaimStatus.Paid;
                            ctx.AddAudit($"Payment executed: {ctx.ClaimCase.ApprovedPayoutAmount:0.00}");
                        }),
                        whenFalse: pipeline => pipeline.AddStep("assign-adjuster", async (ctx, ct) =>
                        {
                            await claimRepository.AssignToAdjusterAsync(ctx.ClaimId, ct);
                            ctx.AddAudit("Claim assigned to adjuster.");
                        })))
            .AddStep("persist-claim-state", async (ctx, ct) =>
            {
                await claimRepository.SaveAsync(ctx, ct);
                ctx.AddAudit("Claim state persisted.");
            })
            .AddStep("send-final-notification", async (ctx, ct) =>
            {
                if (ctx.FinalDecision is not null)
                {
                    await notificationService.NotifyFinalDecisionAsync(ctx.ClaimId, ctx.FinalDecision, ct);
                    ctx.Notifications.Add("Final decision notification sent.");
                }
            })
            .Build();
    }
}
