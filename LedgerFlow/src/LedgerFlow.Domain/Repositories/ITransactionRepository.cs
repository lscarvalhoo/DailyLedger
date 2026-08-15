using LedgerFlow.Domain.Aggregates;

namespace LedgerFlow.Domain.Repositories;

public interface ITransactionRepository
{
    Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Transaction>> GetByMerchantAndDateAsync(Guid merchantId, DateOnly date, CancellationToken cancellationToken = default);

    Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default);
}
