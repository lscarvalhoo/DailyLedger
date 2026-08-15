using LedgerFlow.Application.Abstractions;
using LedgerFlow.Application.DailyBalances.EventHandlers;
using LedgerFlow.Domain.Aggregates;
using LedgerFlow.Domain.Enums;
using LedgerFlow.Domain.Events;
using LedgerFlow.Domain.Repositories;
using NSubstitute;

namespace LedgerFlow.UnitTests.Application.DailyBalances.EventHandlers;

public sealed class TransactionCreatedDailyBalanceHandlerTests
{
    [Fact]
    public async Task Handle_WhenBalanceDoesNotExist_ShouldCreateAndAddBalance()
    {
        var transactionRepository = Substitute.For<ITransactionRepository>();
        var balanceRepository = Substitute.For<IDailyBalanceRepository>();
        var transaction = Transaction.Create(
            Guid.NewGuid(),
            TransactionType.Credit,
            150,
            DateTime.UtcNow,
            "Sale");
        transactionRepository.GetByIdAsync(transaction.Id, Arg.Any<CancellationToken>())
            .Returns(transaction);
        balanceRepository.GetByMerchantAndDateAsync(
                transaction.MerchantId,
                DateOnly.FromDateTime(transaction.OccurredAt),
                Arg.Any<CancellationToken>())
            .Returns((DailyBalance?)null);
        var handler = new TransactionCreatedDailyBalanceHandler(
            transactionRepository,
            balanceRepository);
        var notification = CreateNotification(transaction);

        await handler.Handle(notification, CancellationToken.None);

        await balanceRepository.Received(1).AddAsync(
            Arg.Is<DailyBalance>(balance =>
                balance.MerchantId == transaction.MerchantId &&
                balance.TotalCredits == 150 &&
                balance.Balance == 150),
            CancellationToken.None);
        await balanceRepository.DidNotReceive().UpdateAsync(
            Arg.Any<DailyBalance>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTransactionDoesNotExist_ShouldThrow()
    {
        var transactionRepository = Substitute.For<ITransactionRepository>();
        var balanceRepository = Substitute.For<IDailyBalanceRepository>();
        var transactionId = Guid.NewGuid();
        transactionRepository.GetByIdAsync(transactionId, Arg.Any<CancellationToken>())
            .Returns((Transaction?)null);
        var handler = new TransactionCreatedDailyBalanceHandler(
            transactionRepository,
            balanceRepository);
        var domainEvent = new TransactionCreatedDomainEvent(
            transactionId,
            Guid.NewGuid(),
            TransactionType.Credit,
            100,
            DateTime.UtcNow);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(
            new DomainEventNotification<TransactionCreatedDomainEvent>(domainEvent),
            CancellationToken.None));
    }

    private static DomainEventNotification<TransactionCreatedDomainEvent> CreateNotification(
        Transaction transaction)
    {
        var domainEvent = Assert.IsType<TransactionCreatedDomainEvent>(
            Assert.Single(transaction.DomainEvents));
        return new DomainEventNotification<TransactionCreatedDomainEvent>(domainEvent);
    }
}