using LedgerFlow.Domain.Enums;
using LedgerFlow.Domain.Events;
using LedgerFlow.Infrastructure.Persistence.Context;
using LedgerFlow.Outbox.Messages;
using LedgerFlow.Outbox.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LedgerFlow.UnitTests.Outbox.Persistence.Repositories;

public sealed class OutboxRepositoryTests
{
    [Fact]
    public async Task GetPendingIds_ShouldReturnOnlyUnprocessedMessagesInCreationOrder()
    {
        await using var context = CreateContext();
        var first = CreateMessage();
        var processed = CreateMessage();
        var last = CreateMessage();
        processed.MarkAsProcessed();
        context.OutboxMessages.AddRange(first, processed, last);
        await context.SaveChangesAsync();
        var repository = new OutboxRepository(context);

        var result = await repository.GetPendingIdsAsync(20, CancellationToken.None);

        Assert.Equal([first.Id, last.Id], result);
    }

    [Fact]
    public async Task GetPendingIds_WhenMessageHasManyFailures_ShouldKeepItEligible()
    {
        await using var context = CreateContext();
        var message = CreateMessage();
        for (var attempt = 0; attempt < 10; attempt++)
        {
            message.RegisterFailure();
        }
        context.OutboxMessages.Add(message);
        await context.SaveChangesAsync();
        var repository = new OutboxRepository(context);

        var result = await repository.GetPendingIdsAsync(20, CancellationToken.None);

        Assert.Contains(message.Id, result);
    }

    private static OutboxMessage CreateMessage()
    {
        return OutboxMessage.Create(new TransactionCreatedDomainEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            TransactionType.Credit,
            100,
            DateTime.UtcNow));
    }

    private static LedgerFlowDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<LedgerFlowDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new LedgerFlowDbContext(options);
    }
}