namespace InterviewPrep.Application.Interfaces;

/// <summary>
/// UNIT OF WORK: groups repository changes into one SaveChangesAsync so multiple writes in a single
/// command (e.g. AddAsync a Purchase AND UpgradeTier on the Member) commit as one atomic transaction.
/// Without this, each repository would call SaveChanges itself, and a failure between the two writes
/// could leave data inconsistent.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
