using FluentValidation;
using FluentValidation.Results;
using LedgerFlow.API.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Text.Json;

namespace LedgerFlow.UnitTests.API.Middlewares;

public sealed class ExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_WhenValidationFails_ShouldReturnErrorsByProperty()
    {
        var validationException = new ValidationException([
            new ValidationFailure("Amount", "Amount cannot be negative.")
        ]);
        RequestDelegate next = _ => Task.FromException(validationException);
        var logger = Substitute.For<ILogger<ExceptionHandlingMiddleware>>();
        var middleware = new ExceptionHandlingMiddleware(next, logger);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.Body.Position = 0;
        using var document = await JsonDocument.ParseAsync(context.Response.Body);
        var root = document.RootElement;
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.Equal("Validation failed.", root.GetProperty("title").GetString());
        Assert.Equal(
            "Amount cannot be negative.",
            root.GetProperty("errors").GetProperty("Amount")[0].GetString());
    }
}