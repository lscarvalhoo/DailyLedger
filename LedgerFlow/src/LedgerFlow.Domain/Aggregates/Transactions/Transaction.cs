using LedgerFlow.Domain.Enums;
using LedgerFlow.Domain.Events;
using LedgerFlow.Domain.Exceptions;

namespace LedgerFlow.Domain.Aggregates;

public sealed class Transaction : IHasDomainEvents
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public Guid Id { get; private set; }
    public Guid MerchantId { get; private set; }
    public TransactionType Type { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime OccurredAt { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    private Transaction()
    {
    }

    public static Transaction Create(Guid merchantId, TransactionType type, decimal amount, DateTime occurredAt, string description)
    {
        if (merchantId == Guid.Empty)
        {
            throw new DomainException("MerchantId must be provided.");
        }

        if (amount < 0)
        {
            throw new DomainException("Amount cannot be negative.");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainException("Description must be provided.");
        }

        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            MerchantId = merchantId,
            Type = type,
            Amount = amount,
            OccurredAt = occurredAt,
            Description = description,
            CreatedAt = DateTime.UtcNow
        };

        transaction._domainEvents.Add(new TransactionCreatedDomainEvent(
            transaction.Id, transaction.MerchantId, transaction.Type, transaction.Amount, transaction.OccurredAt));

        return transaction;
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
}
