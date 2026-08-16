using LedgerFlow.Domain.Entities;
using LedgerFlow.Infrastructure.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;

namespace LedgerFlow.UnitTests.Infrastructure.Authentication;

public sealed class AuthenticationServicesTests
{
    [Fact]
    public void PasswordVerifier_ShouldAcceptCorrectPasswordAndRejectInvalidPassword()
    {
        var hasher = new PasswordHasher<User>();
        var target = User.Create(Guid.NewGuid(), "user@test.com", "pending", DateTime.UtcNow);
        var hash = hasher.HashPassword(target, "correct-password");
        var user = User.Create(target.Id, target.Email, hash, target.CreatedAt);
        var verifier = new PasswordVerifier(hasher);

        Assert.True(verifier.Verify(user, "correct-password"));
        Assert.False(verifier.Verify(user, "wrong-password"));
    }

    [Fact]
    public void JwtTokenProvider_ShouldCreateSignedTokenWithUserClaims()
    {
        var options = Options.Create(new JwtOptions
        {
            Issuer = "LedgerFlow.Tests",
            Audience = "LedgerFlow.Tests.Clients",
            Key = "LedgerFlow-Tests-Signing-Key-With-More-Than-32-Characters",
            ExpirationMinutes = 60
        });
        var user = User.Create(Guid.NewGuid(), "user@test.com", "hash", DateTime.UtcNow);
        var provider = new JwtTokenProvider(options);

        var result = provider.Create(user);

        var token = new JwtSecurityTokenHandler().ReadJwtToken(result.AccessToken);
        Assert.Equal("LedgerFlow.Tests", token.Issuer);
        Assert.Contains(token.Claims, claim => claim.Type == JwtRegisteredClaimNames.Email && claim.Value == user.Email);
        Assert.True(result.ExpiresAt > DateTime.UtcNow);
    }
}