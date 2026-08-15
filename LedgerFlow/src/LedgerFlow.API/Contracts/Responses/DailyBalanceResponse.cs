using LedgerFlow.Application.DTOs;

namespace LedgerFlow.API.Contracts.Responses;

public sealed record DailyBalanceResponse(
    Guid Id,
    Guid MerchantId,
    DateOnly Date,
    decimal TotalCredits,
    decimal TotalDebits,
    decimal Balance,
    DateTime UpdatedAt)
{
    public static DailyBalanceResponse From(DailyBalanceDto dailyBalance)
    {
        return new DailyBalanceResponse(
            dailyBalance.Id,
            dailyBalance.MerchantId,
            dailyBalance.Date,
            dailyBalance.TotalCredits,
            dailyBalance.TotalDebits,
            dailyBalance.Balance,
            dailyBalance.UpdatedAt);
    }
}