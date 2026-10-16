using InterviewPrep.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InterviewPrep.Infrastructure.Persistence.Configurations;

public class PurchaseConfiguration : IEntityTypeConfiguration<Purchase>
{
    public void Configure(EntityTypeBuilder<Purchase> builder)
    {
        builder.Property(p => p.Amount).HasColumnType("decimal(18,2)");
        builder.Property(p => p.DiscountAmount).HasColumnType("decimal(18,2)");
        builder.Property(p => p.FinalAmount).HasColumnType("decimal(18,2)");
        builder.Property(p => p.DiscountPercentage).HasColumnType("decimal(5,2)");
    }
}
