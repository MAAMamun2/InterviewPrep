using InterviewPrep.Application.Common.Exceptions;
using InterviewPrep.Application.Interfaces;
using MediatR;

namespace InterviewPrep.Application.Members.Queries.GetMemberById;

public sealed class GetMemberByIdQueryHandler : IRequestHandler<GetMemberByIdQuery, MemberDetailDto>
{
    private readonly IMemberRepository _members;

    public GetMemberByIdQueryHandler(IMemberRepository members)
    {
        _members = members;
    }

    public async Task<MemberDetailDto> Handle(GetMemberByIdQuery request, CancellationToken cancellationToken)
    {
        var member = await _members.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Member), request.Id);

        return new MemberDetailDto(member.Id, member.Name, member.Age, member.Gender.ToString(), member.Tier.ToString());
    }
}
