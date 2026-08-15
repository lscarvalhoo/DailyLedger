using System.Diagnostics;

namespace LedgerFlow.Application.Telemetry;

public static class LedgerFlowTelemetry
{
    public const string ServiceName = "LedgerFlow";
    public const string ActivitySourceName = "LedgerFlow.Application";
    public const string TraceParentHeader = "traceparent";
    public const string TraceStateHeader = "tracestate";

    public static readonly ActivitySource ActivitySource = new(ActivitySourceName);
}