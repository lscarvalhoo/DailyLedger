using LedgerFlow.Application.Abstractions;
using LedgerFlow.Domain.Aggregates;
using LedgerFlow.Domain.Repositories;

namespace LedgerFlow.Application.Transactions.Commands.CreateTransaction;

public sealed class CreateTransactionHandler(
    ITransactionRepository transactionRepository,
    IDomainEventDispatcher domainEventDispatcher) : ICommandHandler<CreateTransactionCommand, Guid>
{
    public async Task<Guid> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = Transaction.Create(
            request.MerchantId,
            request.Type,
            request.Amount,
            request.OccurredAt,
            request.Description!);

        await transactionRepository.AddAsync(transaction, cancellationToken);

        await domainEventDispatcher.DispatchAsync(transaction.DomainEvents, cancellationToken);
        transaction.ClearDomainEvents();

        return transaction.Id;
    }
}
