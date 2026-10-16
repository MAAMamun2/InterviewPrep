using InterviewPrep.Domain.Entities;
using InterviewPrep.Domain.Enums;

namespace InterviewPrep.Domain.Discounts;

public sealed class TierDiscountRule : IDiscountRule
{
    public string Name => "MembershipTier";

    public decimal GetPercentage(Member member, decimal purchaseAmount) => member.Tier switch
    {
        MemberTier.Gold => 10m,
        MemberTier.Platinum => 15m,
        MemberTier.Silver => 5m,
        _ => 0m
    };
}
