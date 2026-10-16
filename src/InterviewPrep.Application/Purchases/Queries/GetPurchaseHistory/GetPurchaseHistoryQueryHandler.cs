using InterviewPrep.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InterviewPrep.Application.Purchases.Queries.GetPurchaseHistory;

public sealed class GetPurchaseHistoryQueryHandler : IRequestHandler<GetPurchaseHistoryQuery, IReadOnlyList<PurchaseDto>>
{
    private readonly IPurchaseRepository _purchases;

    public GetPurchaseHistoryQueryHandler(IPurchaseRepository purchases)
    {
        _purchases = purchases;
    }

    public async Task<IReadOnlyList<PurchaseDto>> Handle(GetPurchaseHistoryQuery request, CancellationToken cancellationToken)
    {
        return await _purchases.Query()
            .Where(p => p.MemberId == request.MemberId)
            .OrderByDescending(p => p.CreatedAtUtc)
            .Select(p => new PurchaseDto(p.Id, p.Amount, p.DiscountAmount, p.FinalAmount, p.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }
}
