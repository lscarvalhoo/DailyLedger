using LedgerFlow.API.Contracts.Responses;
using LedgerFlow.API.Controllers;
using LedgerFlow.Application.DailyBalances.Queries.GetDailyBalance;
using LedgerFlow.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace LedgerFlow.UnitTests.API.Controllers;

public sealed class DailyBalancesControllerTests
{
    [Fact]
    public async Task GetByDate_WhenBalanceExists_ShouldReturnOk()
    {
        var sender = Substitute.For<ISender>();
        var merchantId = Guid.NewGuid();
        var date = new DateOnly(2026, 8, 15);
        var dto = new DailyBalanceDto(
            Guid.NewGuid(), merchantId, date, 200, 50, 150, DateTime.UtcNow);
        sender.Send(Arg.Any<GetDailyBalanceQuery>(), Arg.Any<CancellationToken>())
            .Returns(dto);
        var controller = new DailyBalancesController(sender);

        var result = await controller.GetByDate(merchantId, date, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<DailyBalanceResponse>>(ok.Value);
        Assert.Equal(150, response.Data?.Balance);
    }

    [Fact]
    public async Task GetByDate_WhenBalanceDoesNotExist_ShouldReturnNotFound()
    {
        var sender = Substitute.For<ISender>();
        sender.Send(Arg.Any<GetDailyBalanceQuery>(), Arg.Any<CancellationToken>())
            .Returns((DailyBalanceDto?)null);
        var controller = new DailyBalancesController(sender);

        var result = await controller.GetByDate(
            Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.UtcNow),
            CancellationToken.None);

        var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<DailyBalanceResponse>>(notFound.Value);
        Assert.False(response.Success);
        Assert.Equal("Daily balance not found.", response.Message);
    }
}