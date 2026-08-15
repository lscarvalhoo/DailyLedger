using System.Diagnostics;
using LedgerFlow.Application.Telemetry;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LedgerFlow.Application.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var responseName = typeof(TResponse).Name;
        using var activity = LedgerFlowTelemetry.ActivitySource.StartActivity(
            $"MediatR {requestName}",
            ActivityKind.Internal);

        activity?.SetTag("messaging.system", "mediatr");
        activity?.SetTag("messaging.operation.name", requestName);
        activity?.SetTag("code.function.name", requestName);

        var stopwatch = Stopwatch.StartNew();

        logger.LogInformation(
            "Handling {RequestName}; expected response {ResponseName}",
            requestName,
            responseName);

        try
        {
            var response = await next(cancellationToken);

            activity?.SetStatus(ActivityStatusCode.Ok);
            logger.LogInformation(
                "Handled {RequestName} successfully in {ElapsedMilliseconds} ms",
                requestName,
                stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception exception)
        {
            activity?.SetStatus(ActivityStatusCode.Error, exception.Message);
            activity?.AddEvent(new ActivityEvent(
                "exception",
                tags: new ActivityTagsCollection
                {
                    ["exception.type"] = exception.GetType().FullName,
                    ["exception.message"] = exception.Message
                }));

            logger.LogError(
                exception,
                "Failed handling {RequestName} after {ElapsedMilliseconds} ms",
                requestName,
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }
}