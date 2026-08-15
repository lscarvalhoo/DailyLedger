using LedgerFlow.Application.DTOs;
using LedgerFlow.Domain.Enums;

namespace LedgerFlow.API.Contracts.Responses;

public sealed record TransactionResponse(
    Guid Id,
    Guid MerchantId,
    TransactionType Type,
    decimal Amount,
    DateTime OccurredAt,
    string Description,
    DateTime CreatedAt)
{
    public static TransactionResponse From(TransactionDto transaction)
    {
        return new TransactionResponse(
            transaction.Id,
            transaction.MerchantId,
            transaction.Type,
            transaction.Amount,
            transaction.OccurredAt,
            transaction.Description,
            transaction.CreatedAt);
    }
}