using LedgerFlow.Outbox.Messages;
using LedgerFlow.Outbox.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace LedgerFlow.Outbox.Persistence.Repositories;

public sealed class OutboxRepository(IOutboxDbContext context) : IOutboxRepository
{
    public async Task<IReadOnlyCollection<Guid>> GetPendingIdsAsync(
        int batchSize,
        CancellationToken cancellationToken)
    {
        return await context.OutboxMessages
            .AsNoTracking()
            .Where(message => message.ProcessedAt == null)
            .OrderBy(message => message.CreatedAt)
            .Select(message => message.Id)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken)
    {
        return context.Database.BeginTransactionAsync(cancellationToken);
    }

    public Task<OutboxMessage?> GetForProcessingAsync(Guid messageId, CancellationToken cancellationToken)
    {
        return context.OutboxMessages
            .FromSqlInterpolated($"""
                SELECT *
                FROM OutboxMessages WITH (UPDLOCK, READPAST, ROWLOCK)
                WHERE Id = {messageId} AND ProcessedAt IS NULL
                """)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public Task<OutboxMessage> GetByIdAsync(Guid messageId, CancellationToken cancellationToken)
    {
        return context.OutboxMessages.SingleAsync(
            message => message.Id == messageId,
            cancellationToken);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return context.SaveChangesAsync(cancellationToken);
    }

    public void ClearTracking()
    {
        context.ChangeTracker.Clear();
    }
}