using LedgerFlow.Domain.Enums;

namespace LedgerFlow.Infrastructure.Messaging.RabbitMq.Contracts;

public sealed record TransactionCreated(
    Guid TransactionId,
    Guid MerchantId,
    TransactionType Type,
    decimal Amount,
    DateTime OccurredAt);