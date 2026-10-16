using InterviewPrep.Domain.Entities;

namespace InterviewPrep.Domain.Discounts;

/// <summary>
/// STRATEGY PATTERN. Each discount factor (age/gender/tier) is its own interchangeable strategy
/// behind one interface. POLYMORPHISM: <see cref="DiscountCalculator"/> below only ever calls
/// rule.GetPercentage(...) through this interface -- it never knows or cares which concrete rule
/// it's talking to, and the correct implementation is picked at runtime.
/// </summary>
public interface IDiscountRule
{
    string Name { get; }
    decimal GetPercentage(Member member, decimal purchaseAmount);
}
