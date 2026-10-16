using InterviewPrep.Application.Common;
using InterviewPrep.Application.Interfaces;
using MediatR;

namespace InterviewPrep.Application.Auth.Commands.Login;

/// <summary>
/// "Wrong username/password" is an EXPECTED outcome of a login attempt, not an exceptional one --
/// hence Result&lt;LoginResponse&gt;.Failure(...) instead of throwing. Also deliberately returns the
/// SAME generic error for "user not found" and "wrong password" -- returning a different message
/// for each would let an attacker enumerate valid usernames (a real security point worth raising
/// if asked about authentication).
/// </summary>
public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private const string GenericError = "Invalid username or password.";

    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginCommandHandler(IUserRepository users, IPasswordHasher passwordHasher, IJwtTokenGenerator jwtTokenGenerator)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByUsernameAsync(request.Username, cancellationToken);
        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return Result<LoginResponse>.Failure(GenericError);
        }

        var token = _jwtTokenGenerator.GenerateToken(user);
        return Result<LoginResponse>.Success(new LoginResponse(token, user.Username, user.Role.ToString()));
    }
}
