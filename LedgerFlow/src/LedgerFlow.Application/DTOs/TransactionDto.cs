using LedgerFlow.Domain.Enums;

namespace LedgerFlow.Application.DTOs;

public sealed record TransactionDto(
    Guid Id,
    Guid MerchantId,
    TransactionType Type,
    decimal Amount,
    DateTime OccurredAt,
    string Description,
    DateTime CreatedAt);
