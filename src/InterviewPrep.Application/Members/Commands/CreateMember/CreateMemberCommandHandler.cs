using InterviewPrep.Application.Interfaces;
using InterviewPrep.Domain.Entities;
using MediatR;

namespace InterviewPrep.Application.Members.Commands.CreateMember;

/// <summary>
/// Depends only on IMemberRepository/IUnitOfWork (interfaces) -- never on EF Core or Infrastructure.
/// That's Dependency Inversion again, at the handler level this time.
/// </summary>
public sealed class CreateMemberCommandHandler : IRequestHandler<CreateMemberCommand, int>
{
    private readonly IMemberRepository _members;
    private readonly IUnitOfWork _unitOfWork;

    public CreateMemberCommandHandler(IMemberRepository members, IUnitOfWork unitOfWork)
    {
        _members = members;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(CreateMemberCommand request, CancellationToken cancellationToken)
    {
        var member = Member.Create(request.Name, request.Age, request.Gender);
        await _members.AddAsync(member, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return member.Id;
    }
}
