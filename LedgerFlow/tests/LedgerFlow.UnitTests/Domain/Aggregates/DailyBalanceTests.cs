using LedgerFlow.Domain.Aggregates;
using LedgerFlow.Domain.Enums;
using LedgerFlow.Domain.Exceptions;

namespace LedgerFlow.UnitTests.Domain.Aggregates;

public sealed class DailyBalanceTests
{
    [Fact]
    public void ApplyTransaction_WhenCreditAndDebit_ShouldCalculateBalance()
    {
        var merchantId = Guid.NewGuid();
        var date = new DateOnly(2026, 8, 15);
        var balance = DailyBalance.Create(merchantId, date);

        balance.ApplyTransaction(CreateTransaction(merchantId, date, TransactionType.Credit, 200));
        balance.ApplyTransaction(CreateTransaction(merchantId, date, TransactionType.Debit, 50));

        Assert.Equal(200, balance.TotalCredits);
        Assert.Equal(50, balance.TotalDebits);
        Assert.Equal(150, balance.Balance);
    }

    [Fact]
    public void ApplyTransaction_WhenMerchantDoesNotMatch_ShouldThrow()
    {
        var date = new DateOnly(2026, 8, 15);
        var balance = DailyBalance.Create(Guid.NewGuid(), date);

        Assert.Throws<DomainException>(() => balance.ApplyTransaction(
            CreateTransaction(Guid.NewGuid(), date, TransactionType.Credit, 100)));
    }

    [Fact]
    public void ApplyTransaction_WhenDateDoesNotMatch_ShouldThrow()
    {
        var merchantId = Guid.NewGuid();
        var balance = DailyBalance.Create(merchantId, new DateOnly(2026, 8, 15));

        Assert.Throws<DomainException>(() => balance.ApplyTransaction(
            CreateTransaction(merchantId, new DateOnly(2026, 8, 16), TransactionType.Credit, 100)));
    }

    private static Transaction CreateTransaction(
        Guid merchantId,
        DateOnly date,
        TransactionType type,
        decimal amount)
    {
        return Transaction.Create(
            merchantId,
            type,
            amount,
            date.ToDateTime(new TimeOnly(10, 0)),
            "Test transaction");
    }
}