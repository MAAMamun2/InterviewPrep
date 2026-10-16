using InterviewPrep.Domain.Entities;

namespace InterviewPrep.Application.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}
