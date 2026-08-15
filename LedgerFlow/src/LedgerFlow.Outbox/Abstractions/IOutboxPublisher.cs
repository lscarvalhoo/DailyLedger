using LedgerFlow.Outbox.Messages;

namespace LedgerFlow.Outbox.Abstractions;

public interface IOutboxPublisher
{
    Task PublishAsync(OutboxMessage message, CancellationToken cancellationToken = default);
}
