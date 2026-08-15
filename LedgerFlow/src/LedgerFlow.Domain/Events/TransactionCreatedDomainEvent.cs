using LedgerFlow.Domain.Enums;

namespace LedgerFlow.Domain.Events;

public sealed record TransactionCreatedDomainEvent(
    Guid TransactionId,
    Guid MerchantId,
    TransactionType Type,
    decimal Amount,
    DateTime OccurredAt) : IDomainEvent;
