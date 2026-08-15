using LedgerFlow.API.Contracts.Requests;
using LedgerFlow.API.Contracts.Responses;
using LedgerFlow.API.Controllers;
using LedgerFlow.Application.DTOs;
using LedgerFlow.Application.Transactions.Commands.CreateTransaction;
using LedgerFlow.Application.Transactions.Queries.GetTransaction;
using LedgerFlow.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace LedgerFlow.UnitTests.API.Controllers;

public sealed class TransactionsControllerTests
{
    [Fact]
    public async Task Create_ShouldSendCommandAndReturnCreatedResponse()
    {
        var sender = Substitute.For<ISender>();
        var transactionId = Guid.NewGuid();
        sender.Send(Arg.Any<CreateTransactionCommand>(), Arg.Any<CancellationToken>())
            .Returns(transactionId);
        var controller = new TransactionsController(sender);
        var request = new CreateTransactionRequest(
            Guid.NewGuid(),
            TransactionType.Credit,
            150,
            DateTime.UtcNow,
            "Sale #123");

        var result = await controller.Create(request, CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var response = Assert.IsType<ApiResponse<CreateTransactionResponse>>(created.Value);
        Assert.True(response.Success);
        Assert.Equal(transactionId, response.Data?.Id);
        await sender.Received(1).Send(
            Arg.Is<CreateTransactionCommand>(command =>
                command.MerchantId == request.MerchantId &&
                command.Amount == request.Amount),
            CancellationToken.None);
    }

    [Fact]
    public async Task GetById_WhenTransactionExists_ShouldReturnOk()
    {
        var sender = Substitute.For<ISender>();
        var transactionId = Guid.NewGuid();
        var dto = new TransactionDto(
            transactionId,
            Guid.NewGuid(),
            TransactionType.Debit,
            75,
            DateTime.UtcNow,
            "Payment",
            DateTime.UtcNow);
        sender.Send(Arg.Any<GetTransactionQuery>(), Arg.Any<CancellationToken>())
            .Returns(dto);
        var controller = new TransactionsController(sender);

        var result = await controller.GetById(transactionId, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<TransactionResponse>>(ok.Value);
        Assert.Equal(transactionId, response.Data?.Id);
    }

    [Fact]
    public async Task GetById_WhenTransactionDoesNotExist_ShouldReturnNotFound()
    {
        var sender = Substitute.For<ISender>();
        sender.Send(Arg.Any<GetTransactionQuery>(), Arg.Any<CancellationToken>())
            .Returns((TransactionDto?)null);
        var controller = new TransactionsController(sender);

        var result = await controller.GetById(Guid.NewGuid(), CancellationToken.None);

        var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<TransactionResponse>>(notFound.Value);
        Assert.False(response.Success);
        Assert.Equal("Transaction not found.", response.Message);
    }
}