using LedgerFlow.Application.Abstractions.Authentication;
using LedgerFlow.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace LedgerFlow.Infrastructure.Authentication;

public sealed class PasswordVerifier(IPasswordHasher<User> passwordHasher) : IPasswordVerifier
{
    public bool Verify(User user, string password)
    {
        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}