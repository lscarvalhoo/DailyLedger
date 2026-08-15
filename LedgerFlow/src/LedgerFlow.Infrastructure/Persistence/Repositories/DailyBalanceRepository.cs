using LedgerFlow.Domain.Aggregates;
using LedgerFlow.Domain.Repositories;
using LedgerFlow.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace LedgerFlow.Infrastructure.Persistence.Repositories;

public sealed class DailyBalanceRepository(LedgerFlowDbContext context) : IDailyBalanceRepository
{
    public async Task<DailyBalance?> GetByMerchantAndDateAsync(
        Guid merchantId,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        return await context.DailyBalances.SingleOrDefaultAsync(
            dailyBalance => dailyBalance.MerchantId == merchantId && dailyBalance.Date == date,
            cancellationToken);
    }

    public async Task AddAsync(DailyBalance dailyBalance, CancellationToken cancellationToken = default)
    {
        await context.DailyBalances.AddAsync(dailyBalance, cancellationToken);
    }

    public Task UpdateAsync(DailyBalance dailyBalance, CancellationToken cancellationToken = default)
    {
        context.DailyBalances.Update(dailyBalance);
        return Task.CompletedTask;
    }
}