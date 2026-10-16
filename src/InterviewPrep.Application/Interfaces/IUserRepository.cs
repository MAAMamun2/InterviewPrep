using InterviewPrep.Domain.Entities;

namespace InterviewPrep.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default);
}
