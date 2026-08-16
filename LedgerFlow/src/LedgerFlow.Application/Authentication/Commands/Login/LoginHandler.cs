using LedgerFlow.Application.Abstractions;
using LedgerFlow.Application.Abstractions.Authentication;
using LedgerFlow.Application.DTOs;
using LedgerFlow.Application.Exceptions;
using LedgerFlow.Domain.Repositories;

namespace LedgerFlow.Application.Authentication.Commands.Login;

public sealed class LoginHandler(
    IUserRepository userRepository,
    IPasswordVerifier passwordVerifier,
    IJwtTokenProvider tokenProvider) : ICommandHandler<LoginCommand, AuthenticationDto>
{
    public async Task<AuthenticationDto> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email!.Trim().ToLowerInvariant();
        var user = await userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);

        if (user is null || !passwordVerifier.Verify(user, request.Password!))
        {
            throw new InvalidCredentialsException();
        }

        return tokenProvider.Create(user);
    }
}