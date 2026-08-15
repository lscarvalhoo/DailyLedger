using LedgerFlow.Application.Abstractions;
using LedgerFlow.Infrastructure.Persistence.Context;

namespace LedgerFlow.Infrastructure.Persistence;

public sealed class UnitOfWork(LedgerFlowDbContext context) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return context.SaveChangesAsync(cancellationToken);
    }
}