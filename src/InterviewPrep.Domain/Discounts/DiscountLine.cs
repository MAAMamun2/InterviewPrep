namespace InterviewPrep.Domain.Discounts;

public sealed record DiscountLine(string RuleName, decimal Percentage);

public sealed record DiscountResult(
    IReadOnlyList<DiscountLine> Breakdown,
    decimal TotalPercentage,
    decimal DiscountAmount,
    decimal FinalAmount);
