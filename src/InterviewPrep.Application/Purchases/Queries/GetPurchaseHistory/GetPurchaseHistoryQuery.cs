using MediatR;

namespace InterviewPrep.Application.Purchases.Queries.GetPurchaseHistory;

public sealed record GetPurchaseHistoryQuery(int MemberId) : IRequest<IReadOnlyList<PurchaseDto>>;

public sealed record PurchaseDto(int Id, decimal Amount, decimal DiscountAmount, decimal FinalAmount, DateTime CreatedAtUtc);
