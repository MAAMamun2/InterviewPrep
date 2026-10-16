using FluentAssertions;
using InterviewPrep.Application.Auth.Commands.Login;
using InterviewPrep.Application.Interfaces;
using InterviewPrep.Domain.Entities;
using InterviewPrep.Domain.Enums;
using Moq;
using Xunit;

namespace InterviewPrep.Tests.Application.Auth;

public class LoginCommandHandlerTests
{
    [Fact]
    public async Task Handle_UnknownUsername_ReturnsGenericFailure_NotException()
    {
        var users = new Mock<IUserRepository>();
        users.Setup(u => u.GetByUsernameAsync("ghost", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var handler = new LoginCommandHandler(users.Object, Mock.Of<IPasswordHasher>(), Mock.Of<IJwtTokenGenerator>());

        var result = await handler.Handle(new LoginCommand("ghost", "whatever"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Invalid username or password.");
    }

    [Fact]
    public async Task Handle_WrongPassword_ReturnsSameGenericFailure_AsUnknownUsername()
    {
        var user = User.Create("admin", "hashed", UserRole.Admin);
        var users = new Mock<IUserRepository>();
        users.Setup(u => u.GetByUsernameAsync("admin", It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var hasher = new Mock<IPasswordHasher>();
        hasher.Setup(h => h.Verify("wrong", "hashed")).Returns(false);

        var handler = new LoginCommandHandler(users.Object, hasher.Object, Mock.Of<IJwtTokenGenerator>());

        var result = await handler.Handle(new LoginCommand("admin", "wrong"), CancellationToken.None);

        // Same message as the unknown-username case above -- proves we don't leak which part was wrong.
        result.Error.Should().Be("Invalid username or password.");
    }

    [Fact]
    public async Task Handle_CorrectCredentials_ReturnsTokenAndRole()
    {
        var user = User.Create("admin", "hashed", UserRole.Admin);
        var users = new Mock<IUserRepository>();
        users.Setup(u => u.GetByUsernameAsync("admin", It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var hasher = new Mock<IPasswordHasher>();
        hasher.Setup(h => h.Verify("Admin123!", "hashed")).Returns(true);

        var jwt = new Mock<IJwtTokenGenerator>();
        jwt.Setup(j => j.GenerateToken(user)).Returns("fake-jwt-token");

        var handler = new LoginCommandHandler(users.Object, hasher.Object, jwt.Object);

        var result = await handler.Handle(new LoginCommand("admin", "Admin123!"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Token.Should().Be("fake-jwt-token");
        result.Value.Role.Should().Be("Admin");
    }
}
