using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using LedgerFlow.API.Contracts.Responses;
using LedgerFlow.Domain.Aggregates;
using LedgerFlow.Infrastructure.Persistence.Context;
using LedgerFlow.IntegrationTests.Common;
using LedgerFlow.Outbox.Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LedgerFlow.IntegrationTests.API.Transactions;

[Collection(LedgerFlowApiCollection.Name)]
public sealed class TransactionsEndpointsTests(LedgerFlowApiFactory factory)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    [Fact]
    public async Task PostTransaction_WhenRequestIsValid_ShouldPersistTransactionAndOutbox()
    {
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        var merchantId = Guid.NewGuid();
        var request = new
        {
            merchantId,
            type = "Credit",
            amount = 150m,
            occurredAt = DateTime.UtcNow,
            description = "Sale #123"
        };

        var response = await client.PostAsJsonAsync("/api/transactions", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<CreateTransactionResponse>>();
        Assert.True(body?.Success);
        Assert.NotEqual(Guid.Empty, body?.Data?.Id);

        await using var scope = factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<LedgerFlowDbContext>();
        Assert.Equal(1, await context.Set<Transaction>().CountAsync());
        Assert.Equal(1, await context.Set<OutboxMessage>().CountAsync());
    }

    [Fact]
    public async Task PostTransaction_WhenAmountIsNegative_ShouldReturnPreciseValidationError()
    {
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateClient();
        var request = new
        {
            merchantId = Guid.NewGuid(),
            type = "Credit",
            amount = -150m,
            occurredAt = DateTime.UtcNow,
            description = "Invalid sale"
        };

        var response = await client.PostAsJsonAsync("/api/transactions", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(
            "Amount cannot be negative.",
            document.RootElement.GetProperty("errors").GetProperty("Amount")[0].GetString());
    }

    [Fact]
    public async Task GetTransaction_WhenTransactionExists_ShouldReturnPersistedData()
    {
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateClient();
        var createResponse = await client.PostAsJsonAsync("/api/transactions", new
        {
            merchantId = Guid.NewGuid(),
            type = "Debit",
            amount = 75m,
            occurredAt = DateTime.UtcNow,
            description = "Payment"
        });
        var created = await createResponse.Content
            .ReadFromJsonAsync<ApiResponse<CreateTransactionResponse>>();

        var response = await client.GetAsync($"/api/transactions/{created!.Data!.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<TransactionResponse>>(JsonOptions);
        Assert.Equal(created.Data.Id, body?.Data?.Id);
        Assert.Equal(75, body?.Data?.Amount);
    }

    [Fact]
    public async Task GetTransaction_WhenTransactionDoesNotExist_ShouldReturnNotFound()
    {
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateClient();

        var response = await client.GetAsync($"/api/transactions/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<TransactionResponse>>();
        Assert.False(body?.Success);
        Assert.Equal("Transaction not found.", body?.Message);
    }
}