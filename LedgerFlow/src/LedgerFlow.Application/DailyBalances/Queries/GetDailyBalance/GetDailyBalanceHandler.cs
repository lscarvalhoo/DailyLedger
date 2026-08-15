using LedgerFlow.Application.Abstractions;
using LedgerFlow.Application.DTOs;
using LedgerFlow.Domain.Repositories;

namespace LedgerFlow.Application.DailyBalances.Queries.GetDailyBalance;

public sealed class GetDailyBalanceHandler(IDailyBalanceRepository dailyBalanceRepository)
    : IQueryHandler<GetDailyBalanceQuery, DailyBalanceDto?>
{
    public async Task<DailyBalanceDto?> Handle(GetDailyBalanceQuery request, CancellationToken cancellationToken)
    {
        var dailyBalance = await dailyBalanceRepository.GetByMerchantAndDateAsync(request.MerchantId, request.Date, cancellationToken);

        if (dailyBalance is null)
        {
            return null;
        }

        return new DailyBalanceDto(
            dailyBalance.Id,
            dailyBalance.MerchantId,
            dailyBalance.Date,
            dailyBalance.TotalCredits,
            dailyBalance.TotalDebits,
            dailyBalance.Balance,
            dailyBalance.UpdatedAt);
    }
}
