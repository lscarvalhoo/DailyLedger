using LedgerFlow.Application.DailyBalances.Queries.GetDailyBalance;

namespace LedgerFlow.UnitTests.Application.DailyBalances.Queries.GetDailyBalance;

public sealed class GetDailyBalanceValidatorTests
{
    [Fact]
    public async Task Validate_WhenMerchantAndDateAreEmpty_ShouldReturnBothErrors()
    {
        var validator = new GetDailyBalanceValidator();

        var result = await validator.ValidateAsync(new GetDailyBalanceQuery(Guid.Empty, default));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(GetDailyBalanceQuery.MerchantId));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(GetDailyBalanceQuery.Date));
    }
}