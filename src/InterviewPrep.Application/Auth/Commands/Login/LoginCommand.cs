using InterviewPrep.Application.Common;
using MediatR;

namespace InterviewPrep.Application.Auth.Commands.Login;

public sealed record LoginCommand(string Username, string Password) : IRequest<Result<LoginResponse>>;

public sealed record LoginResponse(string Token, string Username, string Role);
