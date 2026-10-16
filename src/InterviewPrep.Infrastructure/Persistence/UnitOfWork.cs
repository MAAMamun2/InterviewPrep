using InterviewPrep.Application.Interfaces;

namespace InterviewPrep.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly InterviewPrepDbContext _context;

    public UnitOfWork(InterviewPrepDbContext context)
    {
        _context = context;
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _context.SaveChangesAsync(ct);
}
