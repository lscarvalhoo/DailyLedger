using LedgerFlow.Domain.Events;
using System.Diagnostics;
using System.Text.Json;

namespace LedgerFlow.Outbox.Messages;

public sealed class OutboxMessage
{
    public Guid Id { get; private set; }
    public string Type { get; private set; } = string.Empty;
    public string Payload { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public int RetryCount { get; private set; }
    public string? TraceParent { get; private set; }
    public string? TraceState { get; private set; }

    private OutboxMessage()
    {
    }

    public static OutboxMessage Create(IDomainEvent domainEvent)
    {
        var eventType = domainEvent.GetType();

        return new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = eventType.AssemblyQualifiedName
                ?? throw new InvalidOperationException($"Could not resolve type name for {eventType.Name}."),
            Payload = JsonSerializer.Serialize(domainEvent, eventType),
            CreatedAt = DateTime.UtcNow,
            TraceParent = Activity.Current?.Id,
            TraceState = Activity.Current?.TraceStateString
        };
    }

    public void MarkAsProcessed()
    {
        ProcessedAt = DateTime.UtcNow;
    }

    public void RegisterFailure()
    {
        RetryCount++;
    }
}