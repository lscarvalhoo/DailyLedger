using LedgerFlow.Domain.Enums;

namespace LedgerFlow.Domain.Events;

public sealed class TransactionCreatedDomainEvent : IDomainEvent
{
    public Guid TransactionId { get; }
    public Guid MerchantId { get; }
    public TransactionType Type { get; }
    public decimal Amount { get; }
    public DateTime TransactionOccurredAt { get; }
    public DateTime OccurredOn { get; }

    public TransactionCreatedDomainEvent(Guid transactionId, Guid merchantId, TransactionType type, decimal amount, DateTime transactionOccurredAt)
    {
        TransactionId = transactionId;
        MerchantId = merchantId;
        Type = type;
        Amount = amount;
        TransactionOccurredAt = transactionOccurredAt;
        OccurredOn = DateTime.UtcNow;
    }
}
