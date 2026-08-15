using LedgerFlow.Domain.Events;
using MediatR;

namespace LedgerFlow.Application.Abstractions;

public sealed class DomainEventNotification<TDomainEvent>(TDomainEvent domainEvent) : INotification
    where TDomainEvent : IDomainEvent
{
    public TDomainEvent DomainEvent { get; } = domainEvent;
}
