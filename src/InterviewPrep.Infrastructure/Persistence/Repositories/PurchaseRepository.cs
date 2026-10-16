using InterviewPrep.Application.Interfaces;
using InterviewPrep.Domain.Entities;
using InterviewPrep.Infrastructure.Persistence;

namespace InterviewPrep.Infrastructure.Persistence.Repositories;

public class PurchaseRepository : IPurchaseRepository
{
    private readonly InterviewPrepDbContext _context;

    public PurchaseRepository(InterviewPrepDbContext context)
    {
        _context = context;
    }

    public IQueryable<Purchase> Query() => _context.Purchases.AsQueryable();

    public async Task AddAsync(Purchase purchase, CancellationToken ct = default) =>
        await _context.Purchases.AddAsync(purchase, ct);
}
