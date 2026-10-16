using InterviewPrep.Domain.Entities;

namespace InterviewPrep.Domain.Discounts;

/// <summary>
/// DEPENDENCY INJECTION, resolving MANY implementations of ONE interface:
/// the constructor takes IEnumerable&lt;IDiscountRule&gt;, and the DI container hands it every
/// registered rule automatically. This class never does `new AgeDiscountRule()` -- that's
/// Dependency Inversion (the "D" in SOLID): depend on the abstraction, let the container supply
/// the concretions.
/// </summary>
public sealed class DiscountCalculator
{
    private readonly IReadOnlyList<IDiscountRule> _rules;

    public DiscountCalculator(IEnumerable<IDiscountRule> rules)
    {
        _rules = rules.ToList();
    }

    public DiscountResult Calculate(Member member, decimal purchaseAmount)
    {
        if (purchaseAmount <= 0)
            throw new ArgumentOutOfRangeException(nameof(purchaseAmount), "Purchase amount must be positive.");

        var breakdown = _rules
            .Select(rule => new DiscountLine(rule.Name, rule.GetPercentage(member, purchaseAmount)))
            .Where(line => line.Percentage > 0)
            .ToList();

        var totalPercentage = breakdown.Sum(l => l.Percentage);
        var discountAmount = Math.Round(purchaseAmount * totalPercentage / 100m, 2, MidpointRounding.AwayFromZero);
        var finalAmount = purchaseAmount - discountAmount;

        return new DiscountResult(breakdown, totalPercentage, discountAmount, finalAmount);
    }
}
