using LedgerFlow.Application.Abstractions;
using LedgerFlow.Domain.Enums;

namespace LedgerFlow.Application.Transactions.Commands.CreateTransaction;

public sealed record CreateTransactionCommand(
    Guid MerchantId,
    TransactionType Type,
    decimal Amount,
    DateTime OccurredAt,
    string? Description) : ICommand<Guid>;
