using System.ComponentModel.DataAnnotations;
using ClaimFlow.Web.Domain;

namespace ClaimFlow.Web.Models;

public sealed class ClaimRequest
{
    [Required, StringLength(128)]
    public string CustomerName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(128)]
    public string CustomerEmail { get; set; } = string.Empty;

    [Required, StringLength(32)]
    public string PolicyNumber { get; set; } = string.Empty;

    [Required]
    public ClaimType ClaimType { get; set; }

    [Required, StringLength(1024, MinimumLength = 10)]
    public string Description { get; set; } = string.Empty;

    public bool HasPoliceReport { get; set; }
    public bool HasPhotos { get; set; }
    public bool RequiresTowTruck { get; set; }
    public bool InjuryInvolved { get; set; }
    public bool IsVipCustomer { get; set; }
}
