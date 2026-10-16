using FluentAssertions;
using InterviewPrep.Domain.Entities;
using InterviewPrep.Domain.Enums;
using Xunit;

namespace InterviewPrep.Tests.Domain.Entities;

/// <summary>
/// Tests the ENCAPSULATION invariants directly: you literally cannot construct an invalid Member
/// (no public constructor, no public setters) -- these tests prove that by trying the invalid
/// paths through the only door that exists, Create().
/// </summary>
public class MemberTests
{
    [Fact]
    public void Create_WithValidData_Succeeds()
    {
        var member = Member.Create("Alice", 29, Gender.Female);

        member.Name.Should().Be("Alice");
        member.Age.Should().Be(29);
        member.Tier.Should().Be(MemberTier.Silver); // default starting tier
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankName_Throws(string invalidName)
    {
        var act = () => Member.Create(invalidName, 30, Gender.Male);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(121)]
    public void Create_WithImplausibleAge_Throws(int invalidAge)
    {
        var act = () => Member.Create("Bob", invalidAge, Gender.Male);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void UpgradeTier_NeverMovesDown()
    {
        var member = Member.Create("Alice", 29, Gender.Female, MemberTier.Gold);

        member.UpgradeTier(MemberTier.Silver); // attempt to downgrade

        member.Tier.Should().Be(MemberTier.Gold); // unchanged -- the rule a public setter couldn't enforce
    }

    [Fact]
    public void UpgradeTier_MovesUp()
    {
        var member = Member.Create("Alice", 29, Gender.Female, MemberTier.Silver);

        member.UpgradeTier(MemberTier.Platinum);

        member.Tier.Should().Be(MemberTier.Platinum);
    }
}
