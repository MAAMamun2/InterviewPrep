using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using InterviewPrep.Application.Common.Behaviors;
using InterviewPrep.Application.Members.Commands.CreateMember;
using InterviewPrep.Domain.Enums;
using MediatR;
using Moq;
using Xunit;

namespace InterviewPrep.Tests.Application.Common;

/// <summary>
/// Proves the PIPELINE short-circuits: when a validator reports a failure, `next()` -- the actual
/// handler -- is never invoked at all. That's the behavior that saves every handler from writing
/// its own validation boilerplate.
/// </summary>
public class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_WithFailingValidator_ThrowsAndNeverCallsNext()
    {
        var validator = new Mock<IValidator<CreateMemberCommand>>();
        validator
            .Setup(v => v.ValidateAsync(It.IsAny<IValidationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("Name", "Name is required") }));

        var behavior = new ValidationBehavior<CreateMemberCommand, int>(new[] { validator.Object });
        var nextCalled = false;
        RequestHandlerDelegate<int> next = _ => { nextCalled = true; return Task.FromResult(1); };

        var act = async () => await behavior.Handle(
            new CreateMemberCommand("", 30, Gender.Male), next, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        nextCalled.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithNoValidators_CallsNext()
    {
        var behavior = new ValidationBehavior<CreateMemberCommand, int>(Array.Empty<IValidator<CreateMemberCommand>>());
        RequestHandlerDelegate<int> next = _ => Task.FromResult(42);

        var result = await behavior.Handle(
            new CreateMemberCommand("Alice", 30, Gender.Female), next, CancellationToken.None);

        result.Should().Be(42);
    }
}
