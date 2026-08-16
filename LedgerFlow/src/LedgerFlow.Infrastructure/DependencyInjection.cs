using LedgerFlow.Application.Abstractions;
using LedgerFlow.Domain.Repositories;
using LedgerFlow.Infrastructure.Authentication;
using LedgerFlow.Infrastructure.Messaging.RabbitMq;
using LedgerFlow.Infrastructure.Persistence;
using LedgerFlow.Infrastructure.Persistence.Context;
using LedgerFlow.Infrastructure.Persistence.Repositories;
using LedgerFlow.Outbox.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LedgerFlow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException("Connection string 'Database' was not found.");

        services.AddDbContext<LedgerFlowDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IDailyBalanceRepository, DailyBalanceRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IOutboxDbContext>(provider =>
            provider.GetRequiredService<LedgerFlowDbContext>());

        services.AddRabbitMqMessaging(configuration);
        services.AddJwtAuthentication(configuration);

        return services;
    }
}