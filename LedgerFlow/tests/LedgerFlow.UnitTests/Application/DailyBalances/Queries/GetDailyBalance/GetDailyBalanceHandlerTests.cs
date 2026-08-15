using LedgerFlow.Application.DailyBalances.Queries.GetDailyBalance;
using LedgerFlow.Domain.Aggregates;
using LedgerFlow.Domain.Enums;
using LedgerFlow.Domain.Repositories;
using NSubstitute;

namespace LedgerFlow.UnitTests.Application.DailyBalances.Queries.GetDailyBalance;

public sealed class GetDailyBalanceHandlerTests
{
    [Fact]
    public async Task Handle_WhenBalanceExists_ShouldReturnMappedDto()
    {
        var repository = Substitute.For<IDailyBalanceRepository>();
        var merchantId = Guid.NewGuid();
        var date = new DateOnly(2026, 8, 15);
        var balance = DailyBalance.Create(merchantId, date);
        balance.ApplyTransaction(Transaction.Create(
            merchantId,
            TransactionType.Credit,
            200,
            date.ToDateTime(new TimeOnly(10, 0)),
            "Sale"));
        repository.GetByMerchantAndDateAsync(merchantId, date, Arg.Any<CancellationToken>())
            .Returns(balance);
        var handler = new GetDailyBalanceHandler(repository);

        var result = await handler.Handle(
            new GetDailyBalanceQuery(merchantId, date),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(200, result.TotalCredits);
        Assert.Equal(0, result.TotalDebits);
        Assert.Equal(200, result.Balance);
    }

    [Fact]
    public async Task Handle_WhenBalanceDoesNotExist_ShouldReturnNull()
    {
        var repository = Substitute.For<IDailyBalanceRepository>();
        repository.GetByMerchantAndDateAsync(
                Arg.Any<Guid>(),
                Arg.Any<DateOnly>(),
                Arg.Any<CancellationToken>())
            .Returns((DailyBalance?)null);
        var handler = new GetDailyBalanceHandler(repository);

        var result = await handler.Handle(
            new GetDailyBalanceQuery(Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow)),
            CancellationToken.None);

        Assert.Null(result);
    }
}