using LedgerFlow.Application.Telemetry;
using LedgerFlow.Outbox.Abstractions;
using LedgerFlow.Outbox.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace LedgerFlow.Outbox.Processing;

public sealed class OutboxProcessor(
    IServiceScopeFactory scopeFactory,
    ILogger<OutboxProcessor> logger) : BackgroundService
{
    private const int BatchSize = 20;
    private static readonly TimeSpan ProcessingInterval = TimeSpan.FromSeconds(5);

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingMessagesAsync(cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to query pending outbox messages.");
            }

            try
            {
                await Task.Delay(ProcessingInterval, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    private async Task ProcessPendingMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
        var messageIds = await repository.GetPendingIdsAsync(BatchSize, cancellationToken);

        foreach (var messageId in messageIds)
        {
            await ProcessMessageAsync(messageId, cancellationToken);
        }
    }

    private async Task ProcessMessageAsync(Guid messageId, CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
        var publisher = scope.ServiceProvider.GetRequiredService<IOutboxPublisher>();
        Activity? activity = null;

        try
        {
            await using var transaction = await repository.BeginTransactionAsync(cancellationToken);
            var message = await repository.GetForProcessingAsync(messageId, cancellationToken);

            if (message is null)
            {
                return;
            }

            var parentContext = TryParseContext(message.TraceParent, message.TraceState);
            activity = LedgerFlowTelemetry.ActivitySource.StartActivity(
                "Outbox process",
                ActivityKind.Internal,
                parentContext);

            activity?.SetTag("messaging.system", "outbox");
            activity?.SetTag("messaging.message.id", message.Id);
            activity?.SetTag("messaging.message.type", message.Type);
            activity?.SetTag("outbox.retry_count", message.RetryCount);

            logger.LogInformation(
                "Processing Outbox message {OutboxMessageId} of type {MessageType}; retry {RetryCount}",
                message.Id,
                message.Type,
                message.RetryCount);

            await publisher.PublishAsync(message, cancellationToken);

            message.MarkAsProcessed();
            await repository.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
            activity?.SetStatus(ActivityStatusCode.Ok);
            logger.LogInformation("Processed Outbox message {OutboxMessageId}", message.Id);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            activity?.SetStatus(ActivityStatusCode.Error, exception.Message);
            logger.LogError(exception, "Failed to process outbox message {OutboxMessageId}.", messageId);

            repository.ClearTracking();

            var failedMessage = await repository.GetByIdAsync(messageId, cancellationToken);
            failedMessage.RegisterFailure();

            await repository.SaveChangesAsync(cancellationToken);
        }
        finally
        {
            activity?.Dispose();
        }
    }

    private static ActivityContext TryParseContext(string? traceParent, string? traceState)
    {
        return ActivityContext.TryParse(traceParent, traceState, isRemote: true, out var context)
            ? context
            : default;
    }

}