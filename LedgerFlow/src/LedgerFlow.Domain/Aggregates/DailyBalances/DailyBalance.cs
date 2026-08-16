using LedgerFlow.Domain.Enums;
using LedgerFlow.Domain.Exceptions;

namespace LedgerFlow.Domain.Aggregates;

public sealed class DailyBalance
{
    public Guid Id { get; private set; }
    public Guid MerchantId { get; private set; }
    public DateOnly Date { get; private set; }
    public decimal TotalCredits { get; private set; }
    public decimal TotalDebits { get; private set; }
    public decimal Balance { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private DailyBalance()
    {
    }

    public static DailyBalance Create(Guid merchantId, DateOnly date)
    {
        if (merchantId == Guid.Empty)
        {
            throw new DomainException("MerchantId must be provided.");
        }

        return new DailyBalance
        {
            Id = Guid.NewGuid(),
            MerchantId = merchantId,
            Date = date,
            TotalCredits = 0,
            TotalDebits = 0,
            Balance = 0,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void ApplyTransaction(Transaction transaction)
    {
        if (transaction.MerchantId != MerchantId)
        {
            throw new DomainException("Transaction does not belong to this merchant's daily balance.");
        }

        if (DateOnly.FromDateTime(transaction.OccurredAt) != Date)
        {
            throw new DomainException("Transaction date does not match this daily balance's date.");
        }

        switch (transaction.Type)
        {
            case TransactionType.Credit:
                TotalCredits += transaction.Amount;
                break;
            case TransactionType.Debit:
                TotalDebits += transaction.Amount;
                break;
            default:
                throw new DomainException($"Unsupported transaction type: {transaction.Type}.");
        }

        Balance = TotalCredits - TotalDebits;
        UpdatedAt = DateTime.UtcNow;
    }
}
