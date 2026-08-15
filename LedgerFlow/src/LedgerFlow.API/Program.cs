using LedgerFlow.API.Middlewares;
using LedgerFlow.API.Telemetry;
using LedgerFlow.Application;
using LedgerFlow.Infrastructure;
using LedgerFlow.Infrastructure.Persistence;
using LedgerFlow.Outbox;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.AddLedgerFlowObservability();

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddOutbox();

var app = builder.Build();

await app.Services.ApplyDatabaseMigrationsAsync();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "LedgerFlow API v1");
    options.DocumentTitle = "LedgerFlow API";
});

app.UseMiddleware<RequestTraceMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();
