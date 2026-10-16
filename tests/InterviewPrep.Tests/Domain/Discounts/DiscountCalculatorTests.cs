using FluentAssertions;
using InterviewPrep.Domain.Discounts;
using InterviewPrep.Domain.Entities;
using InterviewPrep.Domain.Enums;
using Xunit;

namespace InterviewPrep.Tests.Domain.Discounts;

/// <summary>
/// PURE unit tests -- no mocks, no DI container, no database. DiscountCalculator and the rules
/// are plain objects with no I/O, so they're tested the cheapest, fastest way possible: construct,
/// call, assert. This is the test to point at when asked "what should and shouldn't be unit tested."
/// </summary>
public class DiscountCalculatorTests
{
    private static DiscountCalculator CreateCalculator() => new(new IDiscountRule[]
    {
        new AgeDiscountRule(),
        new GenderDiscountRule(),
        new TierDiscountRule(),
        new BigSpenderDiscountRule()
    });

    [Theory]
    [InlineData(17, 5)]  // boundary: just under 18
    [InlineData(18, 8)]  // boundary: exactly 18 -> next bracket
    [InlineData(24, 8)]  // boundary: just under 25
    [InlineData(25, 10)] // boundary: exactly 25 -> next bracket
    [InlineData(44, 10)] // boundary: just under 45
    [InlineData(45, 0)]  // boundary: exactly 45 -> no age discount
    public void AgeDiscountRule_AppliesCorrectBracket_AtBoundaries(int age, decimal expectedPercentage)
    {
        var member = Member.Create("Test", age, Gender.Male);
        var rule = new AgeDiscountRule();

        rule.GetPercentage(member, 100m).Should().Be(expectedPercentage);
    }

    [Fact]
    public void Calculate_CombinesAllApplicableRules_AndRoundsAwayFromZero()
    {
        // 29yo Female, Gold tier, spending >= 500 -> Age(10) + Gender(8) + Tier(10) + BigSpender(3) = 31%
        var member = Member.Create("Alice", 29, Gender.Female, MemberTier.Gold);
        var calculator = CreateCalculator();

        var result = calculator.Calculate(member, 600m);

        result.TotalPercentage.Should().Be(31m);
        result.DiscountAmount.Should().Be(186m); // 600 * 31% = 186.00
        result.FinalAmount.Should().Be(414m);
        result.Breakdown.Should().HaveCount(4);
    }

    [Fact]
    public void Calculate_ZeroPercentageRules_AreExcludedFromBreakdown()
    {
        // 50yo Silver member spending 100 -> Age(0) + Gender(5, Male) + Tier(5) + BigSpender(0, under 500)
        var member = Member.Create("Bob", 50, Gender.Male, MemberTier.Silver);
        var calculator = CreateCalculator();

        var result = calculator.Calculate(member, 100m);

        result.Breakdown.Select(b => b.RuleName).Should().BeEquivalentTo("Gender", "MembershipTier");
        result.TotalPercentage.Should().Be(10m);
    }

    [Fact]
    public void Calculate_NonPositiveAmount_Throws()
    {
        var member = Member.Create("Bob", 30, Gender.Male);
        var calculator = CreateCalculator();

        var act = () => calculator.Calculate(member, 0m);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
