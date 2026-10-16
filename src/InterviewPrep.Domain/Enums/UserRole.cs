namespace InterviewPrep.Domain.Enums;

// Backs the role claim in the JWT and the [Authorize(Roles = "Admin")] checks in the API.
public enum UserRole
{
    Staff,
    Admin
}
