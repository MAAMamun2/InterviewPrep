using InterviewPrep.Domain.Entities;

namespace InterviewPrep.Application.Interfaces;

/// <summary>
/// ABSTRACTION + DEPENDENCY INVERSION: the Application layer defines this interface and depends
/// only on it. InterviewPrep.Infrastructure implements it with EF Core. Application never
/// references Infrastructure, so you could swap EF Core for Dapper/Mongo without touching a single
/// command/query handler -- that's the point of Clean Architecture's dependency rule
/// (dependencies point inward, toward the domain).
/// </summary>
public interface IMemberRepository
{
    Task<Member?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// Returns IQueryable on purpose: callers (query handlers) compose further .Where()/.OrderBy()
    /// before the Application layer ever materializes anything, so filtering happens in SQL, not
    /// in memory. Contrast with GetByIdAsync, which already knows exactly what it needs and
    /// returns a materialized, awaited Task&lt;Member?&gt; instead.
    /// </summary>
    IQueryable<Member> Query();

    Task AddAsync(Member member, CancellationToken ct = default);
}
