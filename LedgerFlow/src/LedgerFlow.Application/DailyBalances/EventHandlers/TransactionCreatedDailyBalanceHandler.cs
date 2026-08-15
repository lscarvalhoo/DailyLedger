using LedgerFlow.Application.Abstractions;
using LedgerFlow.Domain.Aggregates;
using LedgerFlow.Domain.Events;
using LedgerFlow.Domain.Repositories;
using MediatR;

namespace LedgerFlow.Application.DailyBalances.EventHandlers;

public sealed class TransactionCreatedDailyBalanceHandler(
    ITransactionRepository transactionRepository,
    IDailyBalanceRepository dailyBalanceRepository)
    : INotificationHandler<DomainEventNotification<TransactionCreatedDomainEvent>>
{
    public async Task Handle(DomainEventNotification<TransactionCreatedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var transaction = await transactionRepository.GetByIdAsync(notification.DomainEvent.TransactionId, cancellationToken);

        if (transaction is null)
        {
            throw new InvalidOperationException($"Transaction '{notification.DomainEvent.TransactionId}' was not found.");
        }

        var date = DateOnly.FromDateTime(transaction.OccurredAt);
        var dailyBalance = await dailyBalanceRepository.GetByMerchantAndDateAsync(transaction.MerchantId, date, cancellationToken);

        var isNewDailyBalance = dailyBalance is null;
        dailyBalance ??= DailyBalance.Create(transaction.MerchantId, date);

        dailyBalance.ApplyTransaction(transaction);

        if (isNewDailyBalance)
        {
            await dailyBalanceRepository.AddAsync(dailyBalance, cancellationToken);
        }
        else
        {
            await dailyBalanceRepository.UpdateAsync(dailyBalance, cancellationToken);
        }
    }
}
