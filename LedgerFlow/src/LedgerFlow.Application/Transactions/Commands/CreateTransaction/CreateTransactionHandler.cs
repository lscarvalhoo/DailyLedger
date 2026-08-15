using LedgerFlow.Application.Abstractions;
using LedgerFlow.Domain.Aggregates;
using LedgerFlow.Domain.Repositories;

namespace LedgerFlow.Application.Transactions.Commands.CreateTransaction;

public sealed class CreateTransactionHandler(
    ITransactionRepository transactionRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateTransactionCommand, Guid>
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
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return transaction.Id;
    }
}
