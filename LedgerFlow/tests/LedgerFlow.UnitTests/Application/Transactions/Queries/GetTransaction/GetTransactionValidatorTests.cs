using LedgerFlow.Application.Transactions.Queries.GetTransaction;

namespace LedgerFlow.UnitTests.Application.Transactions.Queries.GetTransaction;

public sealed class GetTransactionValidatorTests
{
    [Fact]
    public async Task Validate_WhenIdIsEmpty_ShouldFail()
    {
        var validator = new GetTransactionValidator();

        var result = await validator.ValidateAsync(new GetTransactionQuery(Guid.Empty));

        var error = Assert.Single(result.Errors);
        Assert.Equal("Transaction id must be provided.", error.ErrorMessage);
    }
}