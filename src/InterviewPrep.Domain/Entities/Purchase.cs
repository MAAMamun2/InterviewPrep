using InterviewPrep.Domain.Common;

namespace InterviewPrep.Domain.Entities;

public class Purchase : BaseEntity
{
    public int MemberId { get; private set; }
    public decimal Amount { get; private set; }
    public decimal DiscountPercentage { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal FinalAmount { get; private set; }

    private Purchase() { }

    public static Purchase Create(int memberId, decimal amount, decimal discountPercentage)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Purchase amount must be positive.");

        var discountAmount = Math.Round(amount * discountPercentage / 100m, 2, MidpointRounding.AwayFromZero);
        return new Purchase
        {
            MemberId = memberId,
            Amount = amount,
            DiscountPercentage = discountPercentage,
            DiscountAmount = discountAmount,
            FinalAmount = amount - discountAmount,
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}
