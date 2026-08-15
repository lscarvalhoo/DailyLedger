using LedgerFlow.Application.Transactions.Commands.CreateTransaction;
using LedgerFlow.Domain.Enums;

namespace LedgerFlow.UnitTests.Application.Transactions.Commands.CreateTransaction;

public sealed class CreateTransactionValidatorTests
{
    [Fact]
    public async Task Validate_WhenCommandIsValid_ShouldSucceed()
    {
        var validator = new CreateTransactionValidator();
        var command = new CreateTransactionCommand(
            Guid.NewGuid(),
            TransactionType.Credit,
            150,
            DateTime.UtcNow,
            "Sale #123");

        var result = await validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-150)]
    public async Task Validate_WhenAmountIsNegative_ShouldReturnPreciseError(decimal amount)
    {
        var validator = new CreateTransactionValidator();
        var command = new CreateTransactionCommand(
            Guid.NewGuid(),
            TransactionType.Credit,
            amount,
            DateTime.UtcNow,
            "Sale");

        var result = await validator.ValidateAsync(command);

        var error = Assert.Single(result.Errors);
        Assert.Equal(nameof(CreateTransactionCommand.Amount), error.PropertyName);
        Assert.Equal("Amount cannot be negative.", error.ErrorMessage);
    }
}