using LedgerFlow.Infrastructure.Messaging.RabbitMq.Configuration;
using LedgerFlow.Infrastructure.Messaging.RabbitMq.Connection;
using LedgerFlow.Infrastructure.Messaging.RabbitMq.Consumers;
using LedgerFlow.Infrastructure.Messaging.RabbitMq.Publishing;
using LedgerFlow.Outbox.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LedgerFlow.Infrastructure.Messaging.RabbitMq;

public static class DependencyInjection
{
    public static IServiceCollection AddRabbitMqMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<RabbitMqOptions>()
            .Bind(configuration.GetSection(RabbitMqOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.HostName), "RabbitMQ HostName is required.")
            .Validate(options => options.Port > 0, "RabbitMQ Port must be greater than zero.")
            .ValidateOnStart();

        services.AddSingleton<RabbitMqConnection>();
        services.AddSingleton<IOutboxPublisher, RabbitMqPublisher>();
        services.AddHostedService<DailyBalanceConsumer>();

        return services;
    }
}