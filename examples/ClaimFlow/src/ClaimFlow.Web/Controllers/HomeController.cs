using System.Text.Json;
using ClaimFlow.Web.Data;
using ClaimFlow.Web.Domain;
using ClaimFlow.Web.Models;
using ClaimFlow.Web.Pipeline;
using ClaimFlow.Web.Services;
using ClaimFlow.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClaimFlow.Web.Controllers;

public sealed class HomeController(
    IClaimRepository claimRepository,
    ClaimProcessingService claimProcessingService,
    AppDbContext dbContext,
    ILogger<HomeController> logger) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var claims = await claimRepository.ListAsync(cancellationToken);
        var policies = await dbContext.Policies.AsNoTracking().OrderBy(x => x.PolicyNumber).ToListAsync(cancellationToken);

        var model = new DashboardViewModel
        {
            Claims = claims.Select(x => new ClaimListItemViewModel
            {
                Id = x.Id,
                CustomerName = x.CustomerName,
                PolicyNumber = x.PolicyNumber,
                ClaimType = x.ClaimType,
                Status = x.Status,
                FinalDecisionType = x.FinalDecisionType,
                ApprovedPayoutAmount = x.ApprovedPayoutAmount,
                CreatedAtUtc = x.CreatedAtUtc
            }).ToList(),
            Policies = policies.Select(x => new DemoPolicyViewModel
            {
                PolicyNumber = x.PolicyNumber,
                IsActive = x.IsActive,
                CoverageLimit = x.CoverageLimit,
                SupportsAutoPayment = x.SupportsAutoPayment
            }).ToList()
        };

        return View(model);
    }

    [HttpGet]
    public IActionResult NewClaim()
    {
        return View(new ClaimRequest
        {
            ClaimType = ClaimType.Kasko,
            HasPhotos = true
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> NewClaim(ClaimRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(request);
        }

        try
        {
            var result = await claimProcessingService.ProcessAsync(request, cancellationToken);
            TempData["Flash"] = $"Claim {result.Claim.Id} processed with status {result.Claim.Status}.";
            return RedirectToAction(nameof(Details), new { id = result.Claim.Id });
        }
        catch (OperationCanceledException)
        {
            TempData["Flash"] = "Claim processing was cancelled.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to process claim.");
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(request);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var claim = await claimRepository.GetAsync(id, cancellationToken);
        if (claim is null)
        {
            return NotFound();
        }

        var latestExecution = claim.Executions.OrderByDescending(x => x.CreatedAtUtc).FirstOrDefault();
        var summary = latestExecution is null
            ? null
            : JsonSerializer.Deserialize<ExecutionSummary>(latestExecution.SummaryJson);

        var model = new ClaimDetailsViewModel
        {
            Claim = claim,
            ExecutionSummary = summary,
            MissingDocuments = ParseJsonArray(claim.MissingDocumentsJson),
            Warnings = ParseJsonArray(claim.WarningsJson)
        };

        return View(model);
    }


    [HttpGet]
    public IActionResult Error()
    {
        return View();
    }

    private static List<string> ParseJsonArray(string json)
        => JsonSerializer.Deserialize<List<string>>(json) ?? [];
}
