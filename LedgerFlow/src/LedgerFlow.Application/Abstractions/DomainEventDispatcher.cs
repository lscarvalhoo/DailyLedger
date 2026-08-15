using System.Diagnostics;
using LedgerFlow.Application.Telemetry;
using LedgerFlow.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LedgerFlow.Application.Abstractions;

public sealed class DomainEventDispatcher(
    IPublisher publisher,
    ILogger<DomainEventDispatcher> logger) : IDomainEventDispatcher
{
    public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            var eventName = domainEvent.GetType().Name;
            using var activity = LedgerFlowTelemetry.ActivitySource.StartActivity(
                $"Domain event {eventName}",
                ActivityKind.Internal);
            activity?.SetTag("domain.event.type", eventName);

            logger.LogInformation("Dispatching domain event {DomainEventType}", eventName);

            var notificationType = typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType());
            var notification = (INotification)Activator.CreateInstance(notificationType, domainEvent)!;

            try
            {
                await publisher.Publish(notification, cancellationToken);
                activity?.SetStatus(ActivityStatusCode.Ok);
                logger.LogInformation("Dispatched domain event {DomainEventType}", eventName);
            }
            catch (Exception exception)
            {
                activity?.SetStatus(ActivityStatusCode.Error, exception.Message);
                logger.LogError(exception, "Failed dispatching domain event {DomainEventType}", eventName);
                throw;
            }
        }
    }
}
