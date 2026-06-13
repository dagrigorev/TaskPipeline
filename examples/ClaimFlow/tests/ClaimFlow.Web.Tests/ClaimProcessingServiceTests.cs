using ClaimFlow.Web.Data;
using ClaimFlow.Web.Domain;
using ClaimFlow.Web.Models;
using ClaimFlow.Web.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ClaimFlow.Web.Tests;

public sealed class ClaimProcessingServiceTests
{
    [Fact]
    public async Task HappyPath_ApprovesAndPaysClaim()
    {
        await using var dbContext = CreateDbContext(nameof(HappyPath_ApprovesAndPaysClaim));
        var service = CreateService(dbContext);

        var request = new ClaimRequest
        {
            CustomerName = "Dmitry",
            CustomerEmail = "dmitry@example.com",
            PolicyNumber = "KSK-1001",
            ClaimType = ClaimType.Kasko,
            Description = "Front bumper and door damage with photos.",
            HasPhotos = true,
            HasPoliceReport = true,
            RequiresTowTruck = false,
            InjuryInvolved = false,
            IsVipCustomer = true
        };

        var result = await service.ProcessAsync(request, CancellationToken.None);

        Assert.Equal(ClaimStatus.Paid, result.Claim.Status);
        Assert.Equal(FinalDecisionType.Approve, result.Claim.FinalDecisionType);
        Assert.NotNull(result.Summary.Root);
    }

    [Fact]
    public async Task MissingDocuments_SendsClaimToWaitingForDocuments()
    {
        await using var dbContext = CreateDbContext(nameof(MissingDocuments_SendsClaimToWaitingForDocuments));
        var service = CreateService(dbContext);

        var request = new ClaimRequest
        {
            CustomerName = "Alex",
            CustomerEmail = "alex@example.com",
            PolicyNumber = "KSK-1001",
            ClaimType = ClaimType.Kasko,
            Description = "Need payout for bumper damage.",
            HasPhotos = false,
            HasPoliceReport = true,
            RequiresTowTruck = false,
            InjuryInvolved = false,
            IsVipCustomer = false
        };

        var result = await service.ProcessAsync(request, CancellationToken.None);

        Assert.Equal(ClaimStatus.WaitingForDocuments, result.Claim.Status);
        Assert.Equal(FinalDecisionType.RequestDocuments, result.Claim.FinalDecisionType);
    }

    [Fact]
    public async Task InactivePolicy_RejectsClaim()
    {
        await using var dbContext = CreateDbContext(nameof(InactivePolicy_RejectsClaim));
        var service = CreateService(dbContext);

        var request = new ClaimRequest
        {
            CustomerName = "Ivan",
            CustomerEmail = "ivan@example.com",
            PolicyNumber = "KSK-9000",
            ClaimType = ClaimType.Kasko,
            Description = "Glass damage with photos.",
            HasPhotos = true,
            HasPoliceReport = true,
            RequiresTowTruck = false,
            InjuryInvolved = false,
            IsVipCustomer = false
        };

        var result = await service.ProcessAsync(request, CancellationToken.None);

        Assert.Equal(ClaimStatus.Rejected, result.Claim.Status);
        Assert.Equal(FinalDecisionType.Reject, result.Claim.FinalDecisionType);
    }

    private static ClaimProcessingService CreateService(AppDbContext dbContext)
    {
        var repository = new ClaimRepository(dbContext);
        var factory = new ClaimPipelineFactory(
            new ClaimValidationService(),
            new PolicyService(dbContext),
            new FraudService(),
            new DocumentService(),
            new DamageEstimator(),
            new RepairQuoteService(),
            new PaymentService(),
            new NotificationService(),
            repository);

        return new ClaimProcessingService(repository, factory);
    }

    private static AppDbContext CreateDbContext(string name)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(name)
            .Options;

        var dbContext = new AppDbContext(options);
        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();
        return dbContext;
    }
}
