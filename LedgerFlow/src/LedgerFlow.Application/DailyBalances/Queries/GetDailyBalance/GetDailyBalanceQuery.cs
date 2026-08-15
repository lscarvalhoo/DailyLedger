using LedgerFlow.Application.Abstractions;
using LedgerFlow.Application.DTOs;

namespace LedgerFlow.Application.DailyBalances.Queries.GetDailyBalance;

public sealed record GetDailyBalanceQuery(Guid MerchantId, DateOnly Date) : IQuery<DailyBalanceDto?>;
