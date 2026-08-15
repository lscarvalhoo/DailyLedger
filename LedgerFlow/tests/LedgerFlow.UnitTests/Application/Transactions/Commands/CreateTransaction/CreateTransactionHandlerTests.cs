using LedgerFlow.Application.Abstractions;
using LedgerFlow.Application.Transactions.Commands.CreateTransaction;
using LedgerFlow.Domain.Aggregates;
using LedgerFlow.Domain.Enums;
using LedgerFlow.Domain.Repositories;
using NSubstitute;

namespace LedgerFlow.UnitTests.Application.Transactions.Commands.CreateTransaction;

public sealed class CreateTransactionHandlerTests
{
    [Fact]
    public async Task Handle_WhenCommandIsValid_ShouldPersistTransactionAndCommit()
    {
        var repository = Substitute.For<ITransactionRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateTransactionHandler(repository, unitOfWork);
        var merchantId = Guid.NewGuid();
        var occurredAt = DateTime.UtcNow;
        var command = new CreateTransactionCommand(
            merchantId,
            TransactionType.Credit,
            150,
            occurredAt,
            "Sale #123");

        var transactionId = await handler.Handle(command, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, transactionId);
        await repository.Received(1).AddAsync(
            Arg.Is<Transaction>(transaction =>
                transaction.Id == transactionId &&
                transaction.MerchantId == merchantId &&
                transaction.Amount == 150 &&
                transaction.OccurredAt == occurredAt),
            CancellationToken.None);
        await unitOfWork.Received(1).SaveChangesAsync(CancellationToken.None);
    }
}