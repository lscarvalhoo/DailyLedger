using System.Diagnostics;

namespace LedgerFlow.API.Middlewares;

public sealed class RequestTraceMiddleware(
    RequestDelegate next,
    ILogger<RequestTraceMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var activity = Activity.Current;
        var stopwatch = Stopwatch.StartNew();

        activity?.SetTag("http.request_id", context.TraceIdentifier);

        using var scope = logger.BeginScope(new Dictionary<string, object?>
        {
            ["RequestId"] = context.TraceIdentifier,
            ["TraceId"] = activity?.TraceId.ToString(),
            ["SpanId"] = activity?.SpanId.ToString()
        });

        logger.LogInformation(
            "HTTP request started {Method} {Path}",
            context.Request.Method,
            context.Request.Path);

        try
        {
            await next(context);

            activity?.SetTag("http.response.status_code", context.Response.StatusCode);
            activity?.SetTag("http.response.body.size", context.Response.ContentLength);

            logger.LogInformation(
                "HTTP response completed {Method} {Path} with {StatusCode} in {ElapsedMilliseconds} ms",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds);
        }
        catch (Exception exception)
        {
            activity?.SetStatus(ActivityStatusCode.Error, exception.Message);
            logger.LogError(
                exception,
                "HTTP request failed {Method} {Path} after {ElapsedMilliseconds} ms",
                context.Request.Method,
                context.Request.Path,
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }
}