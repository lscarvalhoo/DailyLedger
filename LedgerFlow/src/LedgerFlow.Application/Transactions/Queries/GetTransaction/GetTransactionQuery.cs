using LedgerFlow.Application.Abstractions;
using LedgerFlow.Application.DTOs;

namespace LedgerFlow.Application.Transactions.Queries.GetTransaction;

public sealed record GetTransactionQuery(Guid Id) : IQuery<TransactionDto?>;
