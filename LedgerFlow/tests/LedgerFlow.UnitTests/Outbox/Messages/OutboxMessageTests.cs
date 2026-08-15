using System.Diagnostics;
using LedgerFlow.Domain.Enums;
using LedgerFlow.Domain.Events;
using LedgerFlow.Outbox.Messages;

namespace LedgerFlow.UnitTests.Outbox.Messages;

public sealed class OutboxMessageTests
{
    [Fact]
    public void Create_ShouldSerializeDomainEventAndCaptureTraceContext()
    {
        using var activity = new Activity("Outbox message test").Start();
        var domainEvent = new TransactionCreatedDomainEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            TransactionType.Credit,
            150,
            DateTime.UtcNow);

        var message = OutboxMessage.Create(domainEvent);

        Assert.NotEqual(Guid.Empty, message.Id);
        Assert.Equal(typeof(TransactionCreatedDomainEvent).AssemblyQualifiedName, message.Type);
        Assert.Contains(domainEvent.TransactionId.ToString(), message.Payload);
        Assert.Equal(activity.Id, message.TraceParent);
        Assert.Null(message.ProcessedAt);
        Assert.Equal(0, message.RetryCount);
    }

    [Fact]
    public void RegisterFailure_ShouldIncrementRetryCount()
    {
        var message = CreateMessage();

        message.RegisterFailure();
        message.RegisterFailure();

        Assert.Equal(2, message.RetryCount);
    }

    [Fact]
    public void MarkAsProcessed_ShouldSetProcessedAt()
    {
        var message = CreateMessage();

        message.MarkAsProcessed();

        Assert.NotNull(message.ProcessedAt);
    }

    private static OutboxMessage CreateMessage()
    {
        return OutboxMessage.Create(new TransactionCreatedDomainEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            TransactionType.Debit,
            25,
            DateTime.UtcNow));
    }
}