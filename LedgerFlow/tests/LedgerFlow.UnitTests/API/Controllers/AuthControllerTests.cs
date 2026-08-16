using LedgerFlow.API.Contracts.Requests;
using LedgerFlow.API.Contracts.Responses;
using LedgerFlow.API.Controllers;
using LedgerFlow.Application.Authentication.Commands.Login;
using LedgerFlow.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace LedgerFlow.UnitTests.API.Controllers;

public sealed class AuthControllerTests
{
    [Fact]
    public async Task Login_ShouldSendCommandAndReturnTokenResponse()
    {
        var sender = Substitute.For<ISender>();
        var expiresAt = DateTime.UtcNow.AddHours(1);
        sender.Send(Arg.Any<LoginCommand>(), Arg.Any<CancellationToken>())
            .Returns(new AuthenticationDto("jwt-token", expiresAt));
        var controller = new AuthController(sender);
        var request = new LoginRequest("user@example.com", "password");

        var result = await controller.Login(request, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<LoginResponse>>(ok.Value);
        Assert.Equal("jwt-token", response.Data?.AccessToken);
        Assert.Equal("Bearer", response.Data?.TokenType);
        await sender.Received(1).Send(
            Arg.Is<LoginCommand>(command => command.Email == request.Email),
            CancellationToken.None);
    }
}