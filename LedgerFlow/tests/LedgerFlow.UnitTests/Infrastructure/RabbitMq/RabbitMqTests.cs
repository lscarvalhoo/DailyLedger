using LedgerFlow.Domain.Enums;
using LedgerFlow.Infrastructure.Messaging.RabbitMq.Contracts;
using LedgerFlow.Infrastructure.Messaging.RabbitMq.Idempotency;
using LedgerFlow.Infrastructure.Messaging.RabbitMq.Resilience;
using LedgerFlow.Infrastructure.Messaging.RabbitMq.Topology;
using LedgerFlow.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace LedgerFlow.UnitTests.Infrastructure.RabbitMq;

public sealed class RabbitMqTests
{
    [Fact]
    public void TransactionCreated_ShouldRoundTripThroughJson()
    {
        var message = new TransactionCreated(
            Guid.NewGuid(),
            Guid.NewGuid(),
            TransactionType.Credit,
            150,
            DateTime.UtcNow);

        var payload = JsonSerializer.Serialize(message);
        var deserialized = JsonSerializer.Deserialize<TransactionCreated>(payload);

        Assert.Equal(message, deserialized);
    }

    [Fact]
    public void ProcessedMessage_ShouldUseTransactionIdAsUniqueMessageId()
    {
        var transactionId = Guid.NewGuid();

        var processedMessage = ProcessedMessage.Create(transactionId);

        Assert.Equal(transactionId, processedMessage.MessageId);
        Assert.NotEqual(default, processedMessage.ProcessedAt);
    }

    [Fact]
    public void ProcessedMessage_ShouldConfigureMessageIdAsPrimaryKey()
    {
        var options = new DbContextOptionsBuilder<LedgerFlowDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var context = new LedgerFlowDbContext(options);

        var entityType = context.Model.FindEntityType(typeof(ProcessedMessage));
        var primaryKey = entityType?.FindPrimaryKey();

        Assert.NotNull(primaryKey);
        Assert.Equal(nameof(ProcessedMessage.MessageId), Assert.Single(primaryKey.Properties).Name);
    }

    [Theory]
    [InlineData(1, 1_000, RabbitMqTopology.DailyBalanceRetry1Queue)]
    [InlineData(2, 5_000, RabbitMqTopology.DailyBalanceRetry2Queue)]
    [InlineData(3, 30_000, RabbitMqTopology.DailyBalanceRetry3Queue)]
    public void RetryPolicy_ShouldUseExpectedDelayAndQueue(
        int retryCount,
        int expectedDelay,
        string expectedQueue)
    {
        Assert.Equal(expectedDelay, RabbitMqRetryPolicy.GetDelayMilliseconds(retryCount));
        Assert.Equal(expectedQueue, RabbitMqRetryPolicy.GetDestination(retryCount));
    }

    [Fact]
    public void RetryPolicy_AfterThirdRetry_ShouldUseDeadLetterQueue()
    {
        Assert.Equal(
            RabbitMqTopology.DailyBalanceDeadLetterQueue,
            RabbitMqRetryPolicy.GetDestination(RabbitMqRetryPolicy.MaximumRetryCount + 1));
    }

    [Fact]
    public void RetryPolicy_ShouldReadRetryCountHeader()
    {
        var headers = new Dictionary<string, object?>
        {
            [RabbitMqRetryPolicy.RetryCountHeader] = 2
        };

        Assert.Equal(2, RabbitMqRetryPolicy.ReadRetryCount(headers));
    }
}