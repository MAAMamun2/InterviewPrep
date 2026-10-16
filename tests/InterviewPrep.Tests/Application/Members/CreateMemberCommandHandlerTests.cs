using FluentAssertions;
using InterviewPrep.Application.Interfaces;
using InterviewPrep.Application.Members.Commands.CreateMember;
using InterviewPrep.Domain.Entities;
using InterviewPrep.Domain.Enums;
using Moq;
using Xunit;

namespace InterviewPrep.Tests.Application.Members;

/// <summary>
/// MOCKING: the handler depends on IMemberRepository/IUnitOfWork (interfaces), so the test fakes
/// both with Moq instead of hitting a real database. Two things get verified: the RETURN VALUE
/// (member.Id, flowing from the entity created inside Handle), and the INTERACTION (AddAsync and
/// SaveChangesAsync were each called exactly once) -- "state-based" vs "interaction-based" testing,
/// a distinction worth naming if asked to explain this test.
/// </summary>
public class CreateMemberCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_AddsMemberAndSavesOnce()
    {
        var repository = new Mock<IMemberRepository>();
        Member? captured = null;
        repository
            .Setup(r => r.AddAsync(It.IsAny<Member>(), It.IsAny<CancellationToken>()))
            .Callback<Member, CancellationToken>((m, _) => captured = m)
            .Returns(Task.CompletedTask);

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new CreateMemberCommandHandler(repository.Object, unitOfWork.Object);
        var command = new CreateMemberCommand("Charlie", 33, Gender.Male);

        await handler.Handle(command, CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.Name.Should().Be("Charlie");
        repository.Verify(r => r.AddAsync(It.IsAny<Member>(), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
