using FluentValidation;

namespace InterviewPrep.Application.Members.Commands.CreateMember;

public sealed class CreateMemberCommandValidator : AbstractValidator<CreateMemberCommand>
{
    public CreateMemberCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Age).InclusiveBetween(0, 120);
        RuleFor(x => x.Gender).IsInEnum();
    }
}
