using InterviewPrep.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InterviewPrep.Infrastructure.Persistence;

/// <summary>
/// Entity configs are picked up via ApplyConfigurationsFromAssembly instead of configuring inline
/// here -- same convention as HishabPOS's HishabDbContext, so one DbContext file stays readable
/// no matter how many entities the app grows to.
/// </summary>
public class InterviewPrepDbContext : DbContext
{
    public InterviewPrepDbContext(DbContextOptions<InterviewPrepDbContext> options) : base(options) { }

    public DbSet<Member> Members => Set<Member>();
    public DbSet<Purchase> Purchases => Set<Purchase>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InterviewPrepDbContext).Assembly);
    }

    /// <summary>
    /// Auto-populates CreatedAtUtc/UpdatedAtUtc for tracked BaseEntity rows -- the same interceptor-style
    /// convention HishabDbContext uses, just inlined into SaveChangesAsync here for a smaller demo.
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<Domain.Common.BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAtUtc = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
