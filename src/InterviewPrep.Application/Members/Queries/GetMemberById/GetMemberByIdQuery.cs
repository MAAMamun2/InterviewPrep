using MediatR;

namespace InterviewPrep.Application.Members.Queries.GetMemberById;

public sealed record GetMemberByIdQuery(int Id) : IRequest<MemberDetailDto>;

public sealed record MemberDetailDto(int Id, string Name, int Age, string Gender, string Tier);
