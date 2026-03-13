using ClaimFlow.Web.Domain;
using ClaimFlow.Web.Pipeline;

namespace ClaimFlow.Web.ViewModels;

public sealed class DashboardViewModel
{
    public List<ClaimListItemViewModel> Claims { get; set; } = [];
    public List<DemoPolicyViewModel> Policies { get; set; } = [];
}

public sealed class ClaimListItemViewModel
{
    public Guid Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string PolicyNumber { get; set; } = string.Empty;
    public ClaimType ClaimType { get; set; }
    public ClaimStatus Status { get; set; }
    public FinalDecisionType FinalDecisionType { get; set; }
    public decimal ApprovedPayoutAmount { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public sealed class DemoPolicyViewModel
{
    public string PolicyNumber { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public decimal CoverageLimit { get; set; }
    public bool SupportsAutoPayment { get; set; }
}

public sealed class ClaimDetailsViewModel
{
    public ClaimCase Claim { get; set; } = default!;
    public ExecutionSummary? ExecutionSummary { get; set; }
    public List<string> MissingDocuments { get; set; } = [];
    public List<string> Warnings { get; set; } = [];
}
