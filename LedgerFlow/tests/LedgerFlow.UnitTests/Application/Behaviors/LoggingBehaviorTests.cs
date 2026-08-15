using LedgerFlow.Application.Behaviors;
using LedgerFlow.Application.Transactions.Commands.CreateTransaction;
using LedgerFlow.Domain.Enums;
using Microsoft.Extensions.Logging.Abstractions;

namespace LedgerFlow.UnitTests.Application.Behaviors;

public sealed class LoggingBehaviorTests
{
    [Fact]
    public async Task Handle_WhenNextSucceeds_ShouldReturnResponse()
    {
        var behavior = new LoggingBehavior<CreateTransactionCommand, Guid>(
            NullLogger<LoggingBehavior<CreateTransactionCommand, Guid>>.Instance);
        var expected = Guid.NewGuid();

        var result = await behavior.Handle(
            CreateCommand(),
            _ => Task.FromResult(expected),
            CancellationToken.None);

        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task Handle_WhenNextFails_ShouldRethrowException()
    {
        var behavior = new LoggingBehavior<CreateTransactionCommand, Guid>(
            NullLogger<LoggingBehavior<CreateTransactionCommand, Guid>>.Instance);
        var expected = new InvalidOperationException("Test failure");

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => behavior.Handle(
            CreateCommand(),
            _ => Task.FromException<Guid>(expected),
            CancellationToken.None));

        Assert.Same(expected, exception);
    }

    private static CreateTransactionCommand CreateCommand()
    {
        return new CreateTransactionCommand(
            Guid.NewGuid(),
            TransactionType.Credit,
            100,
            DateTime.UtcNow,
            "Test transaction");
    }
}