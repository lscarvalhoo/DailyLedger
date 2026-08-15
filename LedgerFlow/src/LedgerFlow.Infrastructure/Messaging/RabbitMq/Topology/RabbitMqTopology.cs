using LedgerFlow.Infrastructure.Messaging.RabbitMq.Resilience;
using RabbitMQ.Client;

namespace LedgerFlow.Infrastructure.Messaging.RabbitMq.Topology;

public static class RabbitMqTopology
{
    public const string TransactionsExchange = "ledgerflow.transactions";
    public const string DailyBalanceQueue = "daily-balance";
    public const string DailyBalanceRetry1Queue = "daily-balance.retry.1";
    public const string DailyBalanceRetry2Queue = "daily-balance.retry.2";
    public const string DailyBalanceRetry3Queue = "daily-balance.retry.3";
    public const string DailyBalanceDeadLetterQueue = "daily-balance.dlq";
    public const string TransactionCreatedRoutingKey = "transaction.created";

    public static async Task DeclareAsync(IChannel channel, CancellationToken cancellationToken)
    {
        await channel.ExchangeDeclareAsync(
            TransactionsExchange,
            ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            DailyBalanceQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            DailyBalanceQueue,
            TransactionsExchange,
            TransactionCreatedRoutingKey,
            cancellationToken: cancellationToken);

        await DeclareRetryQueueAsync(
            channel,
            DailyBalanceRetry1Queue,
            RabbitMqRetryPolicy.GetDelayMilliseconds(1),
            cancellationToken);
        await DeclareRetryQueueAsync(
            channel,
            DailyBalanceRetry2Queue,
            RabbitMqRetryPolicy.GetDelayMilliseconds(2),
            cancellationToken);
        await DeclareRetryQueueAsync(
            channel,
            DailyBalanceRetry3Queue,
            RabbitMqRetryPolicy.GetDelayMilliseconds(3),
            cancellationToken);

        await channel.QueueDeclareAsync(
            DailyBalanceDeadLetterQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);
    }

    private static async Task DeclareRetryQueueAsync(
        IChannel channel,
        string queueName,
        int delayMilliseconds,
        CancellationToken cancellationToken)
    {
        var arguments = new Dictionary<string, object?>
        {
            ["x-message-ttl"] = delayMilliseconds,
            ["x-dead-letter-exchange"] = TransactionsExchange,
            ["x-dead-letter-routing-key"] = TransactionCreatedRoutingKey
        };

        await channel.QueueDeclareAsync(
            queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments,
            cancellationToken: cancellationToken);
    }
}