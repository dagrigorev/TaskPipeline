using System.Text.Json;
using ClaimFlow.Web.Domain;
using ClaimFlow.Web.Models;
using ClaimFlow.Web.Pipeline;
using TaskPipeline.Abstractions;

namespace ClaimFlow.Web.Services;

public sealed class ClaimProcessingService(
    IClaimRepository claimRepository,
    IClaimPipelineFactory pipelineFactory)
{
    public async Task<(ClaimCase Claim, ExecutionSummary Summary, PipelineExecutionResult PipelineResult)> ProcessAsync(
        ClaimRequest request,
        CancellationToken cancellationToken)
    {
        var claim = new ClaimCase
        {
            CustomerName = request.CustomerName,
            CustomerEmail = request.CustomerEmail,
            PolicyNumber = request.PolicyNumber,
            ClaimType = request.ClaimType,
            Description = request.Description,
            HasPoliceReport = request.HasPoliceReport,
            HasPhotos = request.HasPhotos,
            RequiresTowTruck = request.RequiresTowTruck,
            InjuryInvolved = request.InjuryInvolved,
            IsVipCustomer = request.IsVipCustomer,
            Status = ClaimStatus.Received
        };

        await claimRepository.AddAsync(claim, cancellationToken);

        var context = new ClaimProcessingContext
        {
            ClaimId = claim.Id,
            ClaimCase = claim,
            Request = request
        };

        var pipeline = pipelineFactory.Create();
        var result = await pipeline.ExecuteAsync(context, cancellationToken);
        var summary = ExecutionSummaryMapper.Map(result);

        var snapshot = new ClaimExecutionSnapshot
        {
            ClaimCaseId = claim.Id,
            PipelineName = "claim-processing",
            PipelineStatus = result.Status.ToString(),
            DurationMs = result.Duration.TotalMilliseconds,
            SummaryJson = JsonSerializer.Serialize(summary, new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                WriteIndented = true
            })
        };

        await claimRepository.AddExecutionSnapshotAsync(snapshot, cancellationToken);
        var savedClaim = await claimRepository.GetAsync(claim.Id, cancellationToken) ?? claim;

        return (savedClaim, summary, result);
    }
}
