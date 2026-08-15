using LedgerFlow.Domain.Aggregates;
using LedgerFlow.Domain.Enums;
using LedgerFlow.Domain.Events;
using LedgerFlow.Domain.Exceptions;

namespace LedgerFlow.UnitTests.Domain.Aggregates;

public sealed class TransactionTests
{
    [Fact]
    public void Create_WhenDataIsValid_ShouldCreateTransactionAndDomainEvent()
    {
        var merchantId = Guid.NewGuid();
        var occurredAt = DateTime.UtcNow;

        var transaction = Transaction.Create(
            merchantId,
            TransactionType.Credit,
            150,
            occurredAt,
            "Sale #123");

        Assert.NotEqual(Guid.Empty, transaction.Id);
        Assert.Equal(merchantId, transaction.MerchantId);
        Assert.Equal(150, transaction.Amount);
        var domainEvent = Assert.IsType<TransactionCreatedDomainEvent>(Assert.Single(transaction.DomainEvents));
        Assert.Equal(transaction.Id, domainEvent.TransactionId);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-150)]
    public void Create_WhenAmountIsNegative_ShouldThrow(decimal amount)
    {
        var exception = Assert.Throws<DomainException>(() => Transaction.Create(
            Guid.NewGuid(),
            TransactionType.Credit,
            amount,
            DateTime.UtcNow,
            "Sale"));

        Assert.Equal("Amount cannot be negative.", exception.Message);
    }

    [Fact]
    public void Create_WhenMerchantIdIsEmpty_ShouldThrow()
    {
        Assert.Throws<DomainException>(() => Transaction.Create(
            Guid.Empty,
            TransactionType.Credit,
            100,
            DateTime.UtcNow,
            "Sale"));
    }

    [Fact]
    public void ClearDomainEvents_ShouldRemovePendingEvents()
    {
        var transaction = Transaction.Create(
            Guid.NewGuid(),
            TransactionType.Credit,
            100,
            DateTime.UtcNow,
            "Sale");

        transaction.ClearDomainEvents();

        Assert.Empty(transaction.DomainEvents);
    }
}