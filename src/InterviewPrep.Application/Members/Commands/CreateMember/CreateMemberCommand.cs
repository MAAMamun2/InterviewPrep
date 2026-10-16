using InterviewPrep.Domain.Enums;
using MediatR;

namespace InterviewPrep.Application.Members.Commands.CreateMember;

// CQRS "Command": represents intent to change state, returns only what the caller needs (the new id).
// MediatR's IRequest<int> is what lets InterviewPrep.Api just do `await _mediator.Send(command)`
// without the controller knowing anything about repositories, EF Core, or validation.
public sealed record CreateMemberCommand(string Name, int Age, Gender Gender) : IRequest<int>;
