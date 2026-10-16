using InterviewPrep.Domain.Discounts;
using MediatR;

namespace InterviewPrep.Application.Purchases.Commands.CreatePurchase;

public sealed record CreatePurchaseCommand(int MemberId, decimal Amount) : IRequest<CreatePurchaseResult>;

public sealed record CreatePurchaseResult(
    int PurchaseId,
    decimal Amount,
    IReadOnlyList<DiscountLine> Breakdown,
    decimal TotalDiscountPercentage,
    decimal DiscountAmount,
    decimal FinalAmount);
