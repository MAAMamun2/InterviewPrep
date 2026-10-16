namespace InterviewPrep.Domain.Enums;

// Q: "Why an enum and not magic strings/ints?" -> type safety, compiler-checked switch coverage,
// self-documenting, and EF Core stores it as an int by default (see MemberConfiguration for HasConversion<string>
// if you need the column to be human-readable in the DB -- a common follow-up question).
public enum MemberTier
{
    Silver,
    Gold,
    Platinum
}
