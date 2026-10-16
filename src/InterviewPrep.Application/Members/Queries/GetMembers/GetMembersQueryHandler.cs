using InterviewPrep.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InterviewPrep.Application.Members.Queries.GetMembers;

/// <summary>
/// THE IQueryable&lt;T&gt; vs IEnumerable&lt;T&gt; answer, as real code:
/// `_members.Query()` returns IQueryable&lt;Member&gt;. Every .Where()/.OrderBy()/.Skip()/.Take()
/// below is added to the SAME expression tree -- nothing executes yet. Only `.Select(...).ToListAsync()`
/// at the very end asks EF Core to translate the whole tree into ONE SQL statement and run it.
/// If `Query()` returned IEnumerable&lt;Member&gt; instead, EF Core would have no choice but to pull
/// every row into memory first, then filter/sort/page in C# -- fine for 50 rows, a production
/// incident for 5 million.
/// </summary>
public sealed class GetMembersQueryHandler : IRequestHandler<GetMembersQuery, IReadOnlyList<MemberDto>>
{
    private readonly IMemberRepository _members;

    public GetMembersQueryHandler(IMemberRepository members)
    {
        _members = members;
    }

    public async Task<IReadOnlyList<MemberDto>> Handle(GetMembersQuery request, CancellationToken cancellationToken)
    {
        var query = _members.Query(); // IQueryable<Member> -- nothing has hit the database yet

        if (!string.IsNullOrWhiteSpace(request.NameSearch))
        {
            // Still just building the expression tree; EF Core will translate Contains() to SQL LIKE.
            query = query.Where(m => m.Name.Contains(request.NameSearch));
        }

        return await query
            .OrderBy(m => m.Name)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(m => new MemberDto(
                m.Id, m.Name, m.Age, m.Gender.ToString(), m.Tier.ToString(), m.Purchases.Count))
            .ToListAsync(cancellationToken); // <-- the ONE point where SQL actually runs
    }
}
