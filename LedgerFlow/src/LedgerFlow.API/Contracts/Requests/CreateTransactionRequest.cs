using LedgerFlow.Domain.Enums;

namespace LedgerFlow.API.Contracts.Requests;

public sealed record CreateTransactionRequest(
    Guid MerchantId,
    TransactionType Type,
    decimal Amount,
    DateTime OccurredAt,
    string? Description);