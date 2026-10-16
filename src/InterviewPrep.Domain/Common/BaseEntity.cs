namespace InterviewPrep.Domain.Common;

// INHERITANCE: every entity shares Id/CreatedAtUtc/UpdatedAtUtc through this base class instead of
// redeclaring them. Same convention as HishabPOS's BaseEntity. Setters are public here (not private)
// because EF Core needs to materialize these columns when reading rows -- contrast this with Member.cs,
// which DOES hide its setters, to make the "when is public-setter OK?" nuance concrete.
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}
