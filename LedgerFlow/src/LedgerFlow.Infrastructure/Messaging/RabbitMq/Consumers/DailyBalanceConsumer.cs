using LedgerFlow.Application.Abstractions;
using LedgerFlow.Application.Telemetry;
using LedgerFlow.Domain.Events;
using LedgerFlow.Infrastructure.Messaging.RabbitMq.Connection;
using LedgerFlow.Infrastructure.Messaging.RabbitMq.Contracts;
using LedgerFlow.Infrastructure.Messaging.RabbitMq.Idempotency;
using LedgerFlow.Infrastructure.Messaging.RabbitMq.Resilience;
using LedgerFlow.Infrastructure.Messaging.RabbitMq.Topology;
using LedgerFlow.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace LedgerFlow.Infrastructure.Messaging.RabbitMq.Consumers;

public sealed class DailyBalanceConsumer(
    RabbitMqConnection connection,
    IServiceScopeFactory scopeFactory,
    ILogger<DailyBalanceConsumer> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await ConsumeAsync(cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "RabbitMQ consumer stopped unexpectedly. Retrying.");
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            }
        }
    }

    private async Task ConsumeAsync(CancellationToken cancellationToken)
    {
        var rabbitConnection = await connection.GetConnectionAsync(cancellationToken);
        var channelOptions = new CreateChannelOptions(
            publisherConfirmationsEnabled: true,
            publisherConfirmationTrackingEnabled: true);
        await using var channel = await rabbitConnection.CreateChannelAsync(channelOptions, cancellationToken);

        await RabbitMqTopology.DeclareAsync(channel, cancellationToken);

        await channel.BasicQosAsync(0, 1, global: false, cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, delivery) =>
            await HandleDeliveryAsync(channel, delivery, cancellationToken);

        await channel.BasicConsumeAsync(
            RabbitMqTopology.DailyBalanceQueue,
            autoAck: false,
            consumer,
            cancellationToken);

        await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
    }

    private async Task HandleDeliveryAsync(
        IChannel channel,
        BasicDeliverEventArgs delivery,
        CancellationToken cancellationToken)
    {
        var parentContext = ExtractTraceContext(delivery.BasicProperties.Headers);
        using var activity = LedgerFlowTelemetry.ActivitySource.StartActivity(
            "RabbitMQ consume TransactionCreated",
            ActivityKind.Consumer,
            parentContext);

        activity?.SetTag("messaging.system", "rabbitmq");
        activity?.SetTag("messaging.destination.name", RabbitMqTopology.DailyBalanceQueue);
        activity?.SetTag("messaging.message.id", delivery.BasicProperties.MessageId);
        activity?.SetTag("messaging.operation.type", "process");

        logger.LogInformation(
            "Consuming RabbitMQ message {MessageId} from {Queue}",
            delivery.BasicProperties.MessageId,
            RabbitMqTopology.DailyBalanceQueue);

        try
        {
            var message = JsonSerializer.Deserialize<TransactionCreated>(delivery.Body.Span)
                ?? throw new JsonException("TransactionCreated message is empty.");

            await ProcessMessageAsync(message, cancellationToken);
            await channel.BasicAckAsync(delivery.DeliveryTag, multiple: false, cancellationToken);
            activity?.SetStatus(ActivityStatusCode.Ok);
            logger.LogInformation("Consumed RabbitMQ message {MessageId}", delivery.BasicProperties.MessageId);
        }
        catch (JsonException exception)
        {
            activity?.SetStatus(ActivityStatusCode.Error, exception.Message);
            logger.LogError(exception, "Invalid TransactionCreated message. Moving to DLQ.");
            await MoveToDeadLetterAsync(channel, delivery, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            activity?.SetStatus(ActivityStatusCode.Error, exception.Message);
            logger.LogError(exception, "Failed to consume TransactionCreated message.");
            await RetryOrDeadLetterAsync(channel, delivery, cancellationToken);
        }
    }

    private async Task RetryOrDeadLetterAsync(
        IChannel channel,
        BasicDeliverEventArgs delivery,
        CancellationToken cancellationToken)
    {
        var currentRetryCount = RabbitMqRetryPolicy.ReadRetryCount(delivery.BasicProperties.Headers);
        var nextRetryCount = currentRetryCount + 1;
        var destination = RabbitMqRetryPolicy.GetDestination(nextRetryCount);

        await RepublishAndAckAsync(
            channel,
            delivery,
            destination,
            nextRetryCount,
            cancellationToken);
    }

    private Task MoveToDeadLetterAsync(
        IChannel channel,
        BasicDeliverEventArgs delivery,
        CancellationToken cancellationToken)
    {
        var retryCount = RabbitMqRetryPolicy.ReadRetryCount(delivery.BasicProperties.Headers);

        return RepublishAndAckAsync(
            channel,
            delivery,
            RabbitMqTopology.DailyBalanceDeadLetterQueue,
            retryCount,
            cancellationToken);
    }

    private async Task RepublishAndAckAsync(
        IChannel channel,
        BasicDeliverEventArgs delivery,
        string destination,
        int retryCount,
        CancellationToken cancellationToken)
    {
        try
        {
            var headers = new Dictionary<string, object?>();

            if (delivery.BasicProperties.Headers is not null)
            {
                foreach (var header in delivery.BasicProperties.Headers)
                {
                    headers[header.Key] = header.Value;
                }
            }

            headers[RabbitMqRetryPolicy.RetryCountHeader] = retryCount;
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
                ContentType = delivery.BasicProperties.ContentType ?? "application/json",
                DeliveryMode = DeliveryModes.Persistent,
                MessageId = delivery.BasicProperties.MessageId,
                Type = delivery.BasicProperties.Type,
                Headers = headers
            };

            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: destination,
                mandatory: true,
                basicProperties: properties,
                body: delivery.Body,
                cancellationToken);

            await channel.BasicAckAsync(delivery.DeliveryTag, multiple: false, cancellationToken);

            logger.LogWarning(
                "TransactionCreated message {MessageId} moved to {Destination} after {RetryCount} retries.",
                properties.MessageId,
                destination,
                retryCount);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Could not publish message {MessageId} to {Destination}. Requeueing original delivery.",
                delivery.BasicProperties.MessageId,
                destination);

            await channel.BasicNackAsync(delivery.DeliveryTag, multiple: false, requeue: true, cancellationToken);
        }
    }

    private async Task ProcessMessageAsync(TransactionCreated message, CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LedgerFlowDbContext>();
        var dispatcher = scope.ServiceProvider.GetRequiredService<IDomainEventDispatcher>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        var processedMessages = context.Set<ProcessedMessage>();

        if (await processedMessages.AnyAsync(
            processed => processed.MessageId == message.TransactionId,
            cancellationToken))
        {
            await transaction.CommitAsync(cancellationToken);
            return;
        }

        var domainEvent = new TransactionCreatedDomainEvent(
            message.TransactionId,
            message.MerchantId,
            message.Type,
            message.Amount,
            message.OccurredAt);

        await dispatcher.DispatchAsync([domainEvent], cancellationToken);

        processedMessages.Add(ProcessedMessage.Create(message.TransactionId));
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }

    private static ActivityContext ExtractTraceContext(IDictionary<string, object?>? headers)
    {
        var traceParent = ReadHeader(headers, LedgerFlowTelemetry.TraceParentHeader);
        var traceState = ReadHeader(headers, LedgerFlowTelemetry.TraceStateHeader);

        return ActivityContext.TryParse(traceParent, traceState, isRemote: true, out var context)
            ? context
            : default;
    }

    private static string? ReadHeader(IDictionary<string, object?>? headers, string key)
    {
        if (headers is null || !headers.TryGetValue(key, out var value))
        {
            return null;
        }

        return value switch
        {
            byte[] bytes => Encoding.UTF8.GetString(bytes),
            string text => text,
            _ => value?.ToString()
        };
    }
}