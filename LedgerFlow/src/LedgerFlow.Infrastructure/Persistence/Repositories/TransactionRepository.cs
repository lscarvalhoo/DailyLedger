using LedgerFlow.Domain.Aggregates;
using LedgerFlow.Domain.Repositories;
using LedgerFlow.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace LedgerFlow.Infrastructure.Persistence.Repositories;

public sealed class TransactionRepository(LedgerFlowDbContext context) : ITransactionRepository
{
    public async Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        await context.Transactions.AddAsync(transaction, cancellationToken);
    }

    public async Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var trackedTransaction = context.Transactions.Local.FirstOrDefault(transaction => transaction.Id == id);

        return trackedTransaction ?? await context.Transactions
            .AsNoTracking()
            .SingleOrDefaultAsync(transaction => transaction.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Transaction>> GetByMerchantAndDateAsync(
        Guid merchantId,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        var start = date.ToDateTime(TimeOnly.MinValue);
        var end = start.AddDays(1);

        return await context.Transactions
            .AsNoTracking()
            .Where(transaction =>
                transaction.MerchantId == merchantId &&
                transaction.OccurredAt >= start &&
                transaction.OccurredAt < end)
            .ToListAsync(cancellationToken);
    }
}