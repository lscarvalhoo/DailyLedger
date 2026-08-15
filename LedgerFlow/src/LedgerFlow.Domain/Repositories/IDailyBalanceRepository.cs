using LedgerFlow.Domain.Aggregates;

namespace LedgerFlow.Domain.Repositories;

public interface IDailyBalanceRepository
{
    Task<DailyBalance?> GetByMerchantAndDateAsync(Guid merchantId, DateOnly date, CancellationToken cancellationToken = default);

    Task AddAsync(DailyBalance dailyBalance, CancellationToken cancellationToken = default);

    Task UpdateAsync(DailyBalance dailyBalance, CancellationToken cancellationToken = default);
}
