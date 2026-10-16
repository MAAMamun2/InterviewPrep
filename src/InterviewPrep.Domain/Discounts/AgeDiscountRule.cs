using InterviewPrep.Domain.Entities;

namespace InterviewPrep.Domain.Discounts;

public sealed class AgeDiscountRule : IDiscountRule
{
    public string Name => "Age";

    public decimal GetPercentage(Member member, decimal purchaseAmount) => member.Age switch
    {
        < 18 => 5m,
        < 25 => 8m,
        < 45 => 10m,
        _ => 0m
    };
}
