using FluentValidation;
using LedgerFlow.Application.Behaviors;
using LedgerFlow.Application.Transactions.Commands.CreateTransaction;
using LedgerFlow.Domain.Enums;
using Microsoft.Extensions.Logging.Abstractions;

namespace LedgerFlow.UnitTests.Application.Behaviors;

public sealed class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_WhenRequestIsInvalid_ShouldThrowAndNotCallNext()
    {
        var validator = new CreateTransactionValidator();
        var behavior = new ValidationBehavior<CreateTransactionCommand, Guid>(
            [validator],
            NullLogger<ValidationBehavior<CreateTransactionCommand, Guid>>.Instance);
        var command = new CreateTransactionCommand(
            Guid.Empty,
            (TransactionType)999,
            -1,
            default,
            null);
        var nextCalled = false;

        Task<Guid> Next(CancellationToken _)
        {
            nextCalled = true;
            return Task.FromResult(Guid.NewGuid());
        }

        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => behavior.Handle(command, Next, CancellationToken.None));

        Assert.False(nextCalled);
        Assert.Equal(5, exception.Errors.Count());
    }

    [Fact]
    public async Task Handle_WhenRequestIsValid_ShouldCallNext()
    {
        var validator = new CreateTransactionValidator();
        var behavior = new ValidationBehavior<CreateTransactionCommand, Guid>(
            [validator],
            NullLogger<ValidationBehavior<CreateTransactionCommand, Guid>>.Instance);
        var expectedId = Guid.NewGuid();
        var command = new CreateTransactionCommand(
            Guid.NewGuid(),
            TransactionType.Credit,
            100,
            DateTime.UtcNow,
            "Sale");

        Task<Guid> Next(CancellationToken _) => Task.FromResult(expectedId);

        var result = await behavior.Handle(command, Next, CancellationToken.None);

        Assert.Equal(expectedId, result);
    }
}