using LedgerFlow.Application.Abstractions.Authentication;
using LedgerFlow.Application.Authentication.Commands.Login;
using LedgerFlow.Application.DTOs;
using LedgerFlow.Application.Exceptions;
using LedgerFlow.Domain.Entities;
using LedgerFlow.Domain.Repositories;
using NSubstitute;

namespace LedgerFlow.UnitTests.Application.Authentication.Commands.Login;

public sealed class LoginHandlerTests
{
    [Fact]
    public async Task Handle_WhenCredentialsAreValid_ShouldReturnToken()
    {
        var repository = Substitute.For<IUserRepository>();
        var verifier = Substitute.For<IPasswordVerifier>();
        var tokenProvider = Substitute.For<IJwtTokenProvider>();
        var user = CreateUser();
        var expected = new AuthenticationDto("jwt-token", DateTime.UtcNow.AddHours(1));
        repository.GetByEmailAsync(user.Email, Arg.Any<CancellationToken>()).Returns(user);
        verifier.Verify(user, "password").Returns(true);
        tokenProvider.Create(user).Returns(expected);
        var handler = new LoginHandler(repository, verifier, tokenProvider);

        var result = await handler.Handle(
            new LoginCommand("  USER@EXAMPLE.COM ", "password"),
            CancellationToken.None);

        Assert.Same(expected, result);
        tokenProvider.Received(1).Create(user);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldThrowInvalidCredentials()
    {
        var repository = Substitute.For<IUserRepository>();
        var verifier = Substitute.For<IPasswordVerifier>();
        var tokenProvider = Substitute.For<IJwtTokenProvider>();
        repository.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);
        var handler = new LoginHandler(repository, verifier, tokenProvider);

        await Assert.ThrowsAsync<InvalidCredentialsException>(() => handler.Handle(
            new LoginCommand("user@example.com", "password"),
            CancellationToken.None));

        verifier.DidNotReceive().Verify(Arg.Any<User>(), Arg.Any<string>());
        tokenProvider.DidNotReceive().Create(Arg.Any<User>());
    }

    private static User CreateUser()
    {
        return User.Create(Guid.NewGuid(), "user@example.com", "hash", DateTime.UtcNow);
    }
}