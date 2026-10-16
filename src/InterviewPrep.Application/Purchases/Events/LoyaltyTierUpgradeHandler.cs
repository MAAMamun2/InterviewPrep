using InterviewPrep.Application.Interfaces;
using InterviewPrep.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InterviewPrep.Application.Purchases.Events;

public sealed class LoyaltyTierUpgradeHandler : INotificationHandler<PurchaseCreatedEvent>
{
    private readonly IMemberRepository _members;
    private readonly IPurchaseRepository _purchases;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<LoyaltyTierUpgradeHandler> _logger;

    public LoyaltyTierUpgradeHandler(
        IMemberRepository members,
        IPurchaseRepository purchases,
        IUnitOfWork unitOfWork,
        ILogger<LoyaltyTierUpgradeHandler> logger)
    {
        _members = members;
        _purchases = purchases;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(PurchaseCreatedEvent notification, CancellationToken cancellationToken)
    {
        var member = await _members.GetByIdAsync(notification.MemberId, cancellationToken);
        if (member is null) return;

        var lifetimeSpend = await _purchases.Query()
            .Where(p => p.MemberId == notification.MemberId)
            .SumAsync(p => p.FinalAmount, cancellationToken);

        var newTier = lifetimeSpend switch
        {
            >= 3000m => MemberTier.Platinum,
            >= 1000m => MemberTier.Gold,
            _ => member.Tier
        };

        if (newTier != member.Tier)
        {
            member.UpgradeTier(newTier);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation(
                "Member {MemberId} upgraded to {NewTier} after lifetime spend {LifetimeSpend}",
                member.Id, newTier, lifetimeSpend);
        }
    }
}
