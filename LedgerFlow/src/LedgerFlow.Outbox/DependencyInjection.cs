using LedgerFlow.Outbox.Persistence.Repositories;
using LedgerFlow.Outbox.Processing;
using Microsoft.Extensions.DependencyInjection;

namespace LedgerFlow.Outbox;

public static class DependencyInjection
{
    public static IServiceCollection AddOutbox(this IServiceCollection services)
    {
        services.AddScoped<IOutboxRepository, OutboxRepository>();
        services.AddHostedService<OutboxProcessor>();

        return services;
    }
}