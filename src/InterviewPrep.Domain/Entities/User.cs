using InterviewPrep.Domain.Common;
using InterviewPrep.Domain.Enums;

namespace InterviewPrep.Domain.Entities;

/// <summary>
/// The login principal (staff/admin), kept separate from Member (the loyalty customer being discounted).
/// Separating "who can log in" from "who gets discounts" is itself a common interview question
/// ("how would you model users vs. customers?") -- the answer is: they are different bounded concepts
/// even if in a smaller app you'd be tempted to merge them.
/// </summary>
public class User : BaseEntity
{
    public string Username { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }

    private User() { }

    public static User Create(string username, string passwordHash, UserRole role)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username is required.", nameof(username));

        return new User
        {
            Username = username.Trim(),
            PasswordHash = passwordHash,
            Role = role,
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}
