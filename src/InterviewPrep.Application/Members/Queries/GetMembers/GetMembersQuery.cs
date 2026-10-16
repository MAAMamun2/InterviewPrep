using MediatR;

namespace InterviewPrep.Application.Members.Queries.GetMembers;

public sealed record GetMembersQuery(string? NameSearch, int Page = 1, int PageSize = 20)
    : IRequest<IReadOnlyList<MemberDto>>;

public sealed record MemberDto(int Id, string Name, int Age, string Gender, string Tier, int PurchaseCount);
