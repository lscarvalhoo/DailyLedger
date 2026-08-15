using LedgerFlow.Outbox.Persistence.Repositories;
using LedgerFlow.Outbox.Processing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace LedgerFlow.UnitTests.Outbox.Processing;

public sealed class OutboxProcessorTests
{
    [Fact]
    public async Task StartAsync_ShouldPollPendingMessages()
    {
        var repository = Substitute.For<IOutboxRepository>();
        var polled = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        repository.GetPendingIdsAsync(20, Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                polled.TrySetResult();
                return Task.FromResult<IReadOnlyCollection<Guid>>([]);
            });
        var serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider.GetService(typeof(IOutboxRepository)).Returns(repository);
        var scope = Substitute.For<IServiceScope>();
        scope.ServiceProvider.Returns(serviceProvider);
        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        scopeFactory.CreateScope().Returns(scope);
        var logger = Substitute.For<ILogger<OutboxProcessor>>();
        var processor = new OutboxProcessor(scopeFactory, logger);

        await processor.StartAsync(CancellationToken.None);
        await polled.Task.WaitAsync(TimeSpan.FromSeconds(2));
        await processor.StopAsync(CancellationToken.None);

        await repository.Received().GetPendingIdsAsync(20, Arg.Any<CancellationToken>());
        scope.Received().Dispose();
    }
}