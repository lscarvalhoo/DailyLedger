using LedgerFlow.Application.Abstractions;
using LedgerFlow.Application.DTOs;
using LedgerFlow.Domain.Repositories;

namespace LedgerFlow.Application.Transactions.Queries.GetTransaction;

public sealed class GetTransactionHandler(ITransactionRepository transactionRepository)
    : IQueryHandler<GetTransactionQuery, TransactionDto?>
{
    public async Task<TransactionDto?> Handle(GetTransactionQuery request, CancellationToken cancellationToken)
    {
        var transaction = await transactionRepository.GetByIdAsync(request.Id, cancellationToken);

        if (transaction is null)
        {
            return null;
        }

        return new TransactionDto(
            transaction.Id,
            transaction.MerchantId,
            transaction.Type,
            transaction.Amount,
            transaction.OccurredAt,
            transaction.Description,
            transaction.CreatedAt);
    }
}
