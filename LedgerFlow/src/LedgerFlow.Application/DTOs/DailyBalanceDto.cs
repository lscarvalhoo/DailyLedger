namespace LedgerFlow.Application.DTOs;

public sealed record DailyBalanceDto(
    Guid Id,
    Guid MerchantId,
    DateOnly Date,
    decimal TotalCredits,
    decimal TotalDebits,
    decimal Balance,
    DateTime UpdatedAt);
