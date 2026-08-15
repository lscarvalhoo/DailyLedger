using LedgerFlow.Infrastructure.Messaging.RabbitMq.Topology;

namespace LedgerFlow.Infrastructure.Messaging.RabbitMq.Resilience;

public static class RabbitMqRetryPolicy
{
    public const string RetryCountHeader = "x-retry-count";
    public const int MaximumRetryCount = 3;

    public static int GetDelayMilliseconds(int retryCount)
    {
        return retryCount switch
        {
            1 => 1_000,
            2 => 5_000,
            3 => 30_000,
            _ => throw new ArgumentOutOfRangeException(nameof(retryCount))
        };
    }

    public static string GetDestination(int retryCount)
    {
        return retryCount switch
        {
            1 => RabbitMqTopology.DailyBalanceRetry1Queue,
            2 => RabbitMqTopology.DailyBalanceRetry2Queue,
            3 => RabbitMqTopology.DailyBalanceRetry3Queue,
            _ => RabbitMqTopology.DailyBalanceDeadLetterQueue
        };
    }

    public static int ReadRetryCount(IDictionary<string, object?>? headers)
    {
        if (headers is null || !headers.TryGetValue(RetryCountHeader, out var value))
        {
            return 0;
        }

        return value switch
        {
            byte retryCount => retryCount,
            short retryCount => retryCount,
            int retryCount => retryCount,
            long retryCount => checked((int)retryCount),
            byte[] bytes when int.TryParse(System.Text.Encoding.UTF8.GetString(bytes), out var retryCount) => retryCount,
            _ => 0
        };
    }
}