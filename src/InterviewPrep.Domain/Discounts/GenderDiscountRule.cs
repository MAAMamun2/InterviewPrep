using InterviewPrep.Domain.Entities;
using InterviewPrep.Domain.Enums;

namespace InterviewPrep.Domain.Discounts;

public sealed class GenderDiscountRule : IDiscountRule
{
    public string Name => "Gender";

    public decimal GetPercentage(Member member, decimal purchaseAmount) => member.Gender switch
    {
        Gender.Male => 5m,
        Gender.Female => 8m,
        _ => 0m
    };
}
