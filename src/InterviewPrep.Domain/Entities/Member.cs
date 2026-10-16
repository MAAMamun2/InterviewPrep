using InterviewPrep.Domain.Common;
using InterviewPrep.Domain.Enums;

namespace InterviewPrep.Domain.Entities;

/// <summary>
/// ENCAPSULATION, demonstrated end-to-end:
///  - Fields you must never set directly from outside are `private set` (Name, Age, Gender, Tier).
///  - The only way to create a valid Member is <see cref="Create"/> -- it enforces invariants
///    (name required, age plausible) so an invalid Member can never exist in memory.
///  - The only way to change tier after creation is <see cref="UpgradeTier"/>, which also enforces
///    a rule (can only move up, never down) that a public setter could never enforce.
/// This mirrors the real example from HishabPOS: TenantContext hides its fields behind Resolve()/Clear()
/// instead of letting callers poke _shopId directly. BaseEntity (Id/CreatedAtUtc) is the INHERITANCE example;
/// IMemberRepository (see Application layer) is the ABSTRACTION example; IDiscountRule (many implementations,
/// one interface, chosen at runtime) is the POLYMORPHISM example. One entity, three pillars -- say that line
/// out loud in the interview, it lands well.
/// </summary>
public class Member : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public int Age { get; private set; }
    public Gender Gender { get; private set; }
    public MemberTier Tier { get; private set; }

    private readonly List<Purchase> _purchases = [];
    public IReadOnlyList<Purchase> Purchases => _purchases; // read-only view over a mutable backing list

    private Member() { } // EF Core needs a parameterless ctor; keep it private so app code can't bypass Create().

    public static Member Create(string name, int age, Gender gender, MemberTier startingTier = MemberTier.Silver)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Member name is required.", nameof(name));
        if (age is < 0 or > 120)
            throw new ArgumentOutOfRangeException(nameof(age), "Age must be between 0 and 120.");

        return new Member
        {
            Name = name.Trim(),
            Age = age,
            Gender = gender,
            Tier = startingTier,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public Purchase RecordPurchase(decimal amount, decimal discountPercentage)
    {
        var purchase = Purchase.Create(Id, amount, discountPercentage);
        _purchases.Add(purchase);
        return purchase;
    }

    /// <summary>Tier can only ever move up -- a public `Tier` setter could not guard that rule.</summary>
    public void UpgradeTier(MemberTier newTier)
    {
        if (newTier <= Tier) return;
        Tier = newTier;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
