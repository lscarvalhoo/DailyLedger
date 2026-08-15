using LedgerFlow.Outbox.Messages;
using Microsoft.EntityFrameworkCore.Storage;

namespace LedgerFlow.Outbox.Persistence.Repositories;

public interface IOutboxRepository
{
    Task<IReadOnlyCollection<Guid>> GetPendingIdsAsync(
        int batchSize,
        CancellationToken cancellationToken);

    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken);
    Task<OutboxMessage?> GetForProcessingAsync(Guid messageId, CancellationToken cancellationToken);
    Task<OutboxMessage> GetByIdAsync(Guid messageId, CancellationToken cancellationToken);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    void ClearTracking();
}