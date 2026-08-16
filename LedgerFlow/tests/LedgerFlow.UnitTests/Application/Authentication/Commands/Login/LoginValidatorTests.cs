using LedgerFlow.Application.Authentication.Commands.Login;

namespace LedgerFlow.UnitTests.Application.Authentication.Commands.Login;

public sealed class LoginValidatorTests
{
    [Fact]
    public async Task Validate_WhenEmailAndPasswordAreValid_ShouldSucceed()
    {
        var result = await new LoginValidator().ValidateAsync(
            new LoginCommand("user@example.com", "password"));

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WhenFieldsAreEmpty_ShouldReturnErrors()
    {
        var result = await new LoginValidator().ValidateAsync(new LoginCommand(null, null));

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(LoginCommand.Email));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(LoginCommand.Password));
    }
}