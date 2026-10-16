using InterviewPrep.Domain.Enums;
using InterviewPrep.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace InterviewPrep.Infrastructure.BackgroundJobs;

/// <summary>
/// BACKGROUND SERVICE / HOSTED SERVICE: runs for the entire lifetime of the app as a singleton
/// (registered via AddHostedService in DependencyInjection.cs). The classic interview trap this
/// class exists to demonstrate: a BackgroundService is a SINGLETON, but DbContext is SCOPED --
/// you cannot inject InterviewPrepDbContext directly into this class's constructor (it would be
/// captured once and reused forever, which is wrong and will throw once the first scope disposes).
/// The fix is to inject IServiceScopeFactory and create a fresh scope per tick, which is exactly
/// what's happening below. HishabPOS's real background jobs run through Hangfire instead (which
/// handles this scoping for you + gives retries/persistence/a dashboard) -- this is the lightweight,
/// dependency-free version of the same underlying problem for a demo project.
/// </summary>
public class TierAuditJob : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(30);
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TierAuditJob> _logger;

    public TierAuditJob(IServiceScopeFactory scopeFactory, ILogger<TierAuditJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await AuditTiersAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                // A background job must never crash the whole app on a single bad tick -- log and retry next interval.
                _logger.LogError(ex, "TierAuditJob tick failed");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task AuditTiersAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope(); // fresh scope -> fresh, short-lived DbContext
        var context = scope.ServiceProvider.GetRequiredService<InterviewPrepDbContext>();

        var members = await context.Members.Include(m => m.Purchases).ToListAsync(ct);
        var upgraded = 0;

        foreach (var member in members)
        {
            var lifetimeSpend = member.Purchases.Sum(p => p.FinalAmount);
            var expectedTier = lifetimeSpend switch
            {
                >= 3000m => MemberTier.Platinum,
                >= 1000m => MemberTier.Gold,
                _ => member.Tier
            };

            if (expectedTier != member.Tier)
            {
                member.UpgradeTier(expectedTier);
                upgraded++;
            }
        }

        if (upgraded > 0)
        {
            await context.SaveChangesAsync(ct);
            _logger.LogInformation("TierAuditJob upgraded {Count} member(s)", upgraded);
        }
    }
}
