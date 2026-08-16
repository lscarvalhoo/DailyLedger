using LedgerFlow.Domain.Entities;
using LedgerFlow.Domain.Exceptions;

namespace LedgerFlow.UnitTests.Domain.Entities;

public sealed class UserTests
{
    [Fact]
    public void Create_ShouldNormalizeEmail()
    {
        var user = User.Create(
            Guid.NewGuid(),
            "  User@Test.COM ",
            "password-hash",
            DateTime.UtcNow);

        Assert.Equal("user@test.com", user.Email);
    }

    [Fact]
    public void Create_WhenIdIsEmpty_ShouldThrow()
    {
        Assert.Throws<DomainException>(() => User.Create(
            Guid.Empty,
            "user@test.com",
            "password-hash",
            DateTime.UtcNow));
    }
}