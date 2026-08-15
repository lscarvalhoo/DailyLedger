namespace LedgerFlow.Domain.Events;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}
