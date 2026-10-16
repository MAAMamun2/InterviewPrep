using InterviewPrep.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InterviewPrep.Infrastructure.Persistence.Configurations;

public class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.Property(m => m.Name).IsRequired().HasMaxLength(100);

        // Enums as strings in the DB column -- readable in a SQL client, and inserting a new
        // enum VALUE never shifts existing stored values (inserting a new enum MEMBER in the
        // middle of the C# enum WOULD shift underlying ints if stored as int -- a classic trap).
        builder.Property(m => m.Gender).HasConversion<string>().HasMaxLength(20);
        builder.Property(m => m.Tier).HasConversion<string>().HasMaxLength(20);

        // One-to-many: a Member has many Purchases. EF Core infers the FK (Purchase.MemberId)
        // because of the naming convention; being explicit here documents the relationship intent.
        builder.HasMany(m => m.Purchases)
            .WithOne()
            .HasForeignKey(p => p.MemberId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(m => m.Purchases)
            .UsePropertyAccessMode(PropertyAccessMode.Field); // EF writes straight to the private _purchases field
    }
}
