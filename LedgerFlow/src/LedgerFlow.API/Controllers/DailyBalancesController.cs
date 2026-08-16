using LedgerFlow.API.Contracts.Responses;
using LedgerFlow.Application.DailyBalances.Queries.GetDailyBalance;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LedgerFlow.API.Controllers;

[ApiController]
[Route("api/merchants/{merchantId:guid}/daily-balances")]
[Authorize]
public sealed class DailyBalancesController(ISender sender) : ControllerBase
{
    [HttpGet("{date}")]
    [ProducesResponseType<ApiResponse<DailyBalanceResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<DailyBalanceResponse>>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<DailyBalanceResponse>>> GetByDate(
        Guid merchantId,
        DateOnly date,
        CancellationToken cancellationToken)
    {
        var dailyBalance = await sender.Send(
            new GetDailyBalanceQuery(merchantId, date),
            cancellationToken);

        if (dailyBalance is null)
        {
            return NotFound(ApiResponse<DailyBalanceResponse>.Failure("Daily balance not found."));
        }

        return Ok(ApiResponse<DailyBalanceResponse>.Ok(DailyBalanceResponse.From(dailyBalance)));
    }
}