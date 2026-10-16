using InterviewPrep.Domain.Entities;

namespace InterviewPrep.Domain.Discounts;

/// <summary>
/// OPEN/CLOSED PRINCIPLE, made concrete: this class was added AFTER AgeDiscountRule/GenderDiscountRule/
/// TierDiscountRule existed. Zero lines changed in DiscountCalculator or in any other rule to add it --
/// it's picked up automatically because Program.cs registers every IDiscountRule with
/// `scan.AddClasses(c => c.AssignableTo&lt;IDiscountRule&gt;())` (see DependencyInjection.cs).
/// That's the real answer to "give an example of Open/Closed from your own code."
/// </summary>
public sealed class BigSpenderDiscountRule : IDiscountRule
{
    public string Name => "BigSpender";

    public decimal GetPercentage(Member member, decimal purchaseAmount) => purchaseAmount >= 500m ? 3m : 0m;
}
