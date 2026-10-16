using FluentValidation;

namespace InterviewPrep.Application.Purchases.Commands.CreatePurchase;

public sealed class CreatePurchaseCommandValidator : AbstractValidator<CreatePurchaseCommand>
{
    public CreatePurchaseCommandValidator()
    {
        RuleFor(x => x.MemberId).GreaterThan(0);
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}
