using InterviewPrep.Application.Interfaces;
using InterviewPrep.Domain.Entities;

namespace InterviewPrep.Infrastructure.Persistence.Repositories;

/// <summary>
/// DECORATOR PATTERN: wraps the real MemberRepository and adds caching WITHOUT MemberRepository
/// (or anything calling IMemberRepository) knowing caching exists. Wired up in DependencyInjection.cs
/// with Scrutor's `.Decorate&lt;IMemberRepository, CachedMemberRepository&gt;()` -- the DI container
/// resolves MemberRepository first, then wraps it in this decorator, and everything that asks for
/// IMemberRepository transparently gets the decorated version.
/// Only GetByIdAsync is cached (read-heavy, rarely stale-sensitive for a demo); AddAsync invalidates
/// nothing here on purpose -- it's a deliberately simplified cache-aside example, not production
/// cache invalidation (the honest caveat to raise if asked "would you ship this as-is?": no).
/// </summary>
public class CachedMemberRepository : IMemberRepository
{
    private readonly IMemberRepository _inner;
    private readonly ICacheService _cache;

    public CachedMemberRepository(IMemberRepository inner, ICacheService cache)
    {
        _inner = inner;
        _cache = cache;
    }

    public async Task<Member?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var cacheKey = $"member:{id}";
        if (_cache.TryGet(cacheKey, out Member? cached))
        {
            return cached;
        }

        var member = await _inner.GetByIdAsync(id, ct);
        if (member is not null)
        {
            _cache.Set(cacheKey, member, TimeSpan.FromSeconds(30));
        }

        return member;
    }

    public IQueryable<Member> Query() => _inner.Query(); // not cached: paged/filtered queries aren't cache-friendly keys

    public Task AddAsync(Member member, CancellationToken ct = default) => _inner.AddAsync(member, ct);
}
