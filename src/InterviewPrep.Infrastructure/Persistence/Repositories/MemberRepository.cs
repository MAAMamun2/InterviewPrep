using InterviewPrep.Application.Interfaces;
using InterviewPrep.Domain.Entities;
using InterviewPrep.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InterviewPrep.Infrastructure.Persistence.Repositories;

/// <summary>
/// The concrete EF Core implementation of the Application-layer interface. This is the only class
/// in the solution that knows `DbContext` exists -- that's the Repository pattern doing its job:
/// isolate the ORM/persistence technology behind an interface the rest of the app depends on.
/// </summary>
public class MemberRepository : IMemberRepository
{
    private readonly InterviewPrepDbContext _context;

    public MemberRepository(InterviewPrepDbContext context)
    {
        _context = context;
    }

    public Task<Member?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _context.Members.Include(m => m.Purchases).FirstOrDefaultAsync(m => m.Id == id, ct);

    public IQueryable<Member> Query() => _context.Members.AsQueryable();

    public async Task AddAsync(Member member, CancellationToken ct = default) =>
        await _context.Members.AddAsync(member, ct);
}
