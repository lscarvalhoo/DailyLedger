namespace LedgerFlow.Infrastructure.Messaging.RabbitMq.Idempotency;

public sealed class ProcessedMessage
{
    public Guid MessageId { get; private set; }
    public DateTime ProcessedAt { get; private set; }

    private ProcessedMessage()
    {
    }

    public static ProcessedMessage Create(Guid messageId)
    {
        return new ProcessedMessage
        {
            MessageId = messageId,
            ProcessedAt = DateTime.UtcNow
        };
    }
}