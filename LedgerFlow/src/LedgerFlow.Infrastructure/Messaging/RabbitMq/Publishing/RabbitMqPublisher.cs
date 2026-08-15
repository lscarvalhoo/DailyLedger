using System.Diagnostics;
using System.Text;
using LedgerFlow.Domain.Events;
using LedgerFlow.Application.Telemetry;
using LedgerFlow.Infrastructure.Messaging.RabbitMq.Connection;
using LedgerFlow.Infrastructure.Messaging.RabbitMq.Contracts;
using LedgerFlow.Infrastructure.Messaging.RabbitMq.Topology;
using LedgerFlow.Outbox.Abstractions;
using LedgerFlow.Outbox.Messages;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Text.Json;

namespace LedgerFlow.Infrastructure.Messaging.RabbitMq.Publishing;

public sealed class RabbitMqPublisher(
    RabbitMqConnection connection,
    ILogger<RabbitMqPublisher> logger) : IOutboxPublisher
{
    public async Task PublishAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        using var activity = LedgerFlowTelemetry.ActivitySource.StartActivity(
            "RabbitMQ publish TransactionCreated",
            ActivityKind.Producer);
        activity?.SetTag("messaging.system", "rabbitmq");
        activity?.SetTag("messaging.destination.name", RabbitMqTopology.TransactionsExchange);
        activity?.SetTag("messaging.rabbitmq.destination.routing_key", RabbitMqTopology.TransactionCreatedRoutingKey);
        activity?.SetTag("messaging.message.id", message.Id);

        var domainEvent = JsonSerializer.Deserialize<TransactionCreatedDomainEvent>(message.Payload)
            ?? throw new InvalidOperationException($"Outbox message '{message.Id}' could not be deserialized.");

        var integrationEvent = new TransactionCreated(
            domainEvent.TransactionId,
            domainEvent.MerchantId,
            domainEvent.Type,
            domainEvent.Amount,
            domainEvent.OccurredAt);

        var rabbitConnection = await connection.GetConnectionAsync(cancellationToken);
        var channelOptions = new CreateChannelOptions(
            publisherConfirmationsEnabled: true,
            publisherConfirmationTrackingEnabled: true);

        await using var channel = await rabbitConnection.CreateChannelAsync(channelOptions, cancellationToken);

        await RabbitMqTopology.DeclareAsync(channel, cancellationToken);

        var headers = new Dictionary<string, object?>();
        if (Activity.Current?.Id is { } traceParent)
        {
            headers[LedgerFlowTelemetry.TraceParentHeader] = Encoding.UTF8.GetBytes(traceParent);
        }

        if (Activity.Current?.TraceStateString is { } traceState)
        {
            headers[LedgerFlowTelemetry.TraceStateHeader] = Encoding.UTF8.GetBytes(traceState);
        }

        var properties = new BasicProperties
        {
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent,
            MessageId = message.Id.ToString(),
            Type = nameof(TransactionCreated),
            Headers = headers
        };
        var body = JsonSerializer.SerializeToUtf8Bytes(integrationEvent);

        logger.LogInformation(
            "Publishing RabbitMQ message {MessageId} to {Exchange} with routing key {RoutingKey}",
            message.Id,
            RabbitMqTopology.TransactionsExchange,
            RabbitMqTopology.TransactionCreatedRoutingKey);

        try
        {
            await channel.BasicPublishAsync(
                RabbitMqTopology.TransactionsExchange,
                RabbitMqTopology.TransactionCreatedRoutingKey,
                mandatory: true,
                basicProperties: properties,
                body,
                cancellationToken);

            activity?.SetStatus(ActivityStatusCode.Ok);
            logger.LogInformation("Published RabbitMQ message {MessageId}", message.Id);
        }
        catch (Exception exception)
        {
            activity?.SetStatus(ActivityStatusCode.Error, exception.Message);
            logger.LogError(exception, "Failed publishing RabbitMQ message {MessageId}", message.Id);
            throw;
        }
    }
}