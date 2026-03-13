using ClaimFlow.Web.Domain;
using Microsoft.EntityFrameworkCore;

namespace ClaimFlow.Web.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<ClaimCase> Claims => Set<ClaimCase>();
    public DbSet<ClaimExecutionSnapshot> Executions => Set<ClaimExecutionSnapshot>();
    public DbSet<PolicyRecord> Policies => Set<PolicyRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ClaimCase>()
            .HasMany(x => x.Executions)
            .WithOne(x => x.ClaimCase)
            .HasForeignKey(x => x.ClaimCaseId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PolicyRecord>()
            .HasIndex(x => x.PolicyNumber)
            .IsUnique();

        modelBuilder.Entity<PolicyRecord>().HasData(
            new PolicyRecord
            {
                Id = 1,
                PolicyNumber = "KSK-1001",
                IsActive = true,
                CoverageLimit = 7000m,
                CoversGlass = true,
                CoversTowTruck = true,
                SupportsAutoPayment = true
            },
            new PolicyRecord
            {
                Id = 2,
                PolicyNumber = "OSG-2001",
                IsActive = true,
                CoverageLimit = 2500m,
                CoversGlass = false,
                CoversTowTruck = false,
                SupportsAutoPayment = true
            },
            new PolicyRecord
            {
                Id = 3,
                PolicyNumber = "KSK-9000",
                IsActive = false,
                CoverageLimit = 10000m,
                CoversGlass = true,
                CoversTowTruck = true,
                SupportsAutoPayment = false
            });
    }
}
