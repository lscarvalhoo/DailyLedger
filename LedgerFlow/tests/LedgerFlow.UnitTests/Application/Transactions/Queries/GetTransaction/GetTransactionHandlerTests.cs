using LedgerFlow.Application.Transactions.Queries.GetTransaction;
using LedgerFlow.Domain.Aggregates;
using LedgerFlow.Domain.Enums;
using LedgerFlow.Domain.Repositories;
using NSubstitute;

namespace LedgerFlow.UnitTests.Application.Transactions.Queries.GetTransaction;

public sealed class GetTransactionHandlerTests
{
    [Fact]
    public async Task Handle_WhenTransactionExists_ShouldReturnMappedDto()
    {
        var repository = Substitute.For<ITransactionRepository>();
        var transaction = Transaction.Create(
            Guid.NewGuid(),
            TransactionType.Debit,
            75,
            DateTime.UtcNow,
            "Supplier payment");
        repository.GetByIdAsync(transaction.Id, Arg.Any<CancellationToken>())
            .Returns(transaction);
        var handler = new GetTransactionHandler(repository);

        var result = await handler.Handle(new GetTransactionQuery(transaction.Id), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(transaction.Id, result.Id);
        Assert.Equal(transaction.MerchantId, result.MerchantId);
        Assert.Equal(transaction.Type, result.Type);
        Assert.Equal(transaction.Amount, result.Amount);
    }

    [Fact]
    public async Task Handle_WhenTransactionDoesNotExist_ShouldReturnNull()
    {
        var repository = Substitute.For<ITransactionRepository>();
        repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Transaction?)null);
        var handler = new GetTransactionHandler(repository);

        var result = await handler.Handle(new GetTransactionQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Null(result);
    }
}