using LedgerFlow.API.Contracts.Requests;
using LedgerFlow.API.Contracts.Responses;
using LedgerFlow.Application.Transactions.Commands.CreateTransaction;
using LedgerFlow.Application.Transactions.Queries.GetTransaction;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LedgerFlow.API.Controllers;

[ApiController]
[Route("api/transactions")]
public sealed class TransactionsController(ISender sender) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<ApiResponse<CreateTransactionResponse>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<CreateTransactionResponse>>> Create(
        CreateTransactionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateTransactionCommand(
            request.MerchantId,
            request.Type,
            request.Amount,
            request.OccurredAt,
            request.Description);

        var id = await sender.Send(command, cancellationToken);
        var response = ApiResponse<CreateTransactionResponse>.Ok(
            CreateTransactionResponse.From(id));

        return CreatedAtAction(nameof(GetById), new { id }, response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<ApiResponse<TransactionResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<TransactionResponse>>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TransactionResponse>>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var transaction = await sender.Send(new GetTransactionQuery(id), cancellationToken);

        if (transaction is null)
        {
            return NotFound(ApiResponse<TransactionResponse>.Failure("Transaction not found."));
        }

        return Ok(ApiResponse<TransactionResponse>.Ok(TransactionResponse.From(transaction)));
    }
}