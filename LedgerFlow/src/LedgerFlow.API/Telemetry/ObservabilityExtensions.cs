using LedgerFlow.Application.Telemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace LedgerFlow.API.Telemetry;

public static class ObservabilityExtensions
{
    public static WebApplicationBuilder AddLedgerFlowObservability(this WebApplicationBuilder builder)
    {
        var resource = ResourceBuilder.CreateDefault()
            .AddService(
                LedgerFlowTelemetry.ServiceName,
                serviceVersion: typeof(Program).Assembly.GetName().Version?.ToString());

        builder.Logging.ClearProviders();
        builder.Logging.Configure(options =>
        {
            options.ActivityTrackingOptions =
                ActivityTrackingOptions.TraceId |
                ActivityTrackingOptions.SpanId |
                ActivityTrackingOptions.ParentId;
        });

        builder.Logging.AddOpenTelemetry(options =>
        {
            options.SetResourceBuilder(resource);
            options.IncludeScopes = true;
            options.IncludeFormattedMessage = true;
            options.ParseStateValues = true;
            options.AddConsoleExporter();
        });

        builder.Services
            .AddOpenTelemetry()
            .ConfigureResource(resourceBuilder => resourceBuilder.AddService(
                LedgerFlowTelemetry.ServiceName,
                serviceVersion: typeof(Program).Assembly.GetName().Version?.ToString()))
            .WithTracing(tracing => tracing
                .AddSource(LedgerFlowTelemetry.ActivitySourceName)
                .AddAspNetCoreInstrumentation(options => options.RecordException = true)
                .AddHttpClientInstrumentation(options => options.RecordException = true)
                .AddEntityFrameworkCoreInstrumentation()
                .AddConsoleExporter());

        return builder;
    }
}