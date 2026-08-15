using System.Diagnostics;
using System.Text.Json;
using LedgerFlow.Domain.Aggregates;
using LedgerFlow.Domain.Enums;
using LedgerFlow.Domain.Events;
using LedgerFlow.Infrastructure.Persistence.Context;
using LedgerFlow.Outbox.Messages;
using Microsoft.EntityFrameworkCore;

namespace LedgerFlow.UnitTests.Outbox.Persistence.Context;

public sealed class OutboxPersistenceTests
{
    [Fact]
    public async Task SaveChanges_WhenAggregateHasDomainEvent_ShouldPersistOutboxMessageAndClearEvents()
    {
        using var parentActivity = new Activity("Create transaction test").Start();
        await using var context = CreateContext();
        var transaction = Transaction.Create(
            Guid.NewGuid(),
            TransactionType.Credit,
            150,
            DateTime.UtcNow,
            "Sale #123");
        context.Transactions.Add(transaction);

        await context.SaveChangesAsync();

        var outboxMessage = await context.OutboxMessages.SingleAsync();
        var domainEvent = JsonSerializer.Deserialize<TransactionCreatedDomainEvent>(outboxMessage.Payload);
        Assert.Equal(transaction.Id, domainEvent?.TransactionId);
        Assert.Equal(parentActivity.Id, outboxMessage.TraceParent);
        Assert.Empty(transaction.DomainEvents);
    }

    [Fact]
    public async Task SaveChanges_WhenAggregateHasNoDomainEvents_ShouldNotPersistOutboxMessage()
    {
        await using var context = CreateContext();
        var transaction = Transaction.Create(
            Guid.NewGuid(),
            TransactionType.Credit,
            100,
            DateTime.UtcNow,
            "Sale");
        transaction.ClearDomainEvents();
        context.Transactions.Add(transaction);

        await context.SaveChangesAsync();

        Assert.Empty(await context.OutboxMessages.ToListAsync());
    }

    private static LedgerFlowDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<LedgerFlowDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new LedgerFlowDbContext(options);
    }
}