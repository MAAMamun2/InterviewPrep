using InterviewPrep.Application.Interfaces;
using InterviewPrep.Domain.Entities;
using InterviewPrep.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InterviewPrep.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly InterviewPrepDbContext _context;

    public UserRepository(InterviewPrepDbContext context)
    {
        _context = context;
    }

    // Parameterized via EF Core's LINQ translation -- NOT string-concatenated SQL, so this is
    // immune to SQL injection by construction. Good to point at when asked "how do you prevent SQLi?"
    public Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default) =>
        _context.Users.FirstOrDefaultAsync(u => u.Username == username, ct);
}
