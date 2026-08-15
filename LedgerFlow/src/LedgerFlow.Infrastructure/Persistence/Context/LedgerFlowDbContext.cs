using LedgerFlow.Domain.Aggregates;
using LedgerFlow.Domain.Events;
using LedgerFlow.Infrastructure.Messaging.RabbitMq.Idempotency;
using LedgerFlow.Outbox;
using LedgerFlow.Outbox.Configurations;
using LedgerFlow.Outbox.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LedgerFlow.Infrastructure.Persistence.Context;

public sealed class LedgerFlowDbContext(DbContextOptions<LedgerFlowDbContext> options)
    : DbContext(options), IOutboxDbContext
{
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<DailyBalance> DailyBalances => Set<DailyBalance>();
    public DbSet<ProcessedMessage> ProcessedMessages => Set<ProcessedMessage>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var aggregates = ChangeTracker
            .Entries<IHasDomainEvents>()
            .Select(entry => entry.Entity)
            .Where(aggregate => aggregate.DomainEvents.Count > 0)
            .ToList();

        var outboxMessages = aggregates
            .SelectMany(aggregate => aggregate.DomainEvents)
            .Select(OutboxMessage.Create)
            .ToList();

        OutboxMessages.AddRange(outboxMessages);

        var result = await base.SaveChangesAsync(cancellationToken);

        foreach (var aggregate in aggregates)
        {
            aggregate.ClearDomainEvents();
        }

        return result;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LedgerFlowDbContext).Assembly);
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
    }
}