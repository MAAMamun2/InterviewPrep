using InterviewPrep.Application.Common.Exceptions;
using InterviewPrep.Application.Interfaces;
using InterviewPrep.Application.Purchases.Events;
using InterviewPrep.Domain.Discounts;
using MediatR;

namespace InterviewPrep.Application.Purchases.Commands.CreatePurchase;

public sealed class CreatePurchaseCommandHandler : IRequestHandler<CreatePurchaseCommand, CreatePurchaseResult>
{
    private readonly IMemberRepository _members;
    private readonly IPurchaseRepository _purchases;
    private readonly IUnitOfWork _unitOfWork;
    private readonly DiscountCalculator _discountCalculator;
    private readonly IPublisher _publisher; // MediatR's publish-only interface (vs ISender for Send)

    public CreatePurchaseCommandHandler(
        IMemberRepository members,
        IPurchaseRepository purchases,
        IUnitOfWork unitOfWork,
        DiscountCalculator discountCalculator,
        IPublisher publisher)
    {
        _members = members;
        _purchases = purchases;
        _unitOfWork = unitOfWork;
        _discountCalculator = discountCalculator;
        _publisher = publisher;
    }

    public async Task<CreatePurchaseResult> Handle(CreatePurchaseCommand request, CancellationToken cancellationToken)
    {
        var member = await _members.GetByIdAsync(request.MemberId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Member), request.MemberId);

        var discount = _discountCalculator.Calculate(member, request.Amount);
        var purchase = member.RecordPurchase(request.Amount, discount.TotalPercentage);

        await _purchases.AddAsync(purchase, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Published AFTER SaveChangesAsync: the upgrade handler queries lifetime spend including
        // this purchase, so ordering here isn't incidental -- it's load-bearing.
        await _publisher.Publish(new PurchaseCreatedEvent(member.Id, purchase.Id, purchase.FinalAmount), cancellationToken);

        return new CreatePurchaseResult(
            purchase.Id, request.Amount, discount.Breakdown, discount.TotalPercentage,
            discount.DiscountAmount, discount.FinalAmount);
    }
}
