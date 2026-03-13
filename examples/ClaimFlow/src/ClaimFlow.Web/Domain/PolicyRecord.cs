using System.ComponentModel.DataAnnotations;

namespace ClaimFlow.Web.Domain;

public sealed class PolicyRecord
{
    public int Id { get; set; }

    [MaxLength(32)]
    public string PolicyNumber { get; set; } = string.Empty;

    public bool IsActive { get; set; }
    public decimal CoverageLimit { get; set; }
    public bool CoversGlass { get; set; }
    public bool CoversTowTruck { get; set; }
    public bool SupportsAutoPayment { get; set; }
}
