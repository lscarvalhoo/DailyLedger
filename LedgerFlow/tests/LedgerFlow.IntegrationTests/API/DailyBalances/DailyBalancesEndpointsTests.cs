using LedgerFlow.API.Contracts.Responses;
using LedgerFlow.Domain.Aggregates;
using LedgerFlow.Domain.Enums;
using LedgerFlow.Infrastructure.Persistence.Context;
using LedgerFlow.IntegrationTests.Common;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace LedgerFlow.IntegrationTests.API.DailyBalances;

[Collection(LedgerFlowApiCollection.Name)]
public sealed class DailyBalancesEndpointsTests(LedgerFlowApiFactory factory)
{
    [Fact]
    public async Task GetDailyBalance_WhenBalanceExists_ShouldReturnTotals()
    {
        await factory.ResetDatabaseAsync();
        var merchantId = Guid.NewGuid();
        var date = new DateOnly(2026, 8, 15);
        await SeedBalanceAsync(merchantId, date);
        using var client = await factory.CreateAuthenticatedClientAsync();

        var response = await client.GetAsync(
            $"/api/merchants/{merchantId}/daily-balances/{date:yyyy-MM-dd}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<DailyBalanceResponse>>();
        Assert.Equal(200, body?.Data?.TotalCredits);
        Assert.Equal(50, body?.Data?.TotalDebits);
        Assert.Equal(150, body?.Data?.Balance);
    }

    [Fact]
    public async Task GetDailyBalance_WhenBalanceDoesNotExist_ShouldReturnNotFound()
    {
        await factory.ResetDatabaseAsync();
        using var client = await factory.CreateAuthenticatedClientAsync();

        var response = await client.GetAsync(
            $"/api/merchants/{Guid.NewGuid()}/daily-balances/2026-08-15");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task SeedBalanceAsync(Guid merchantId, DateOnly date)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<LedgerFlowDbContext>();
        var balance = DailyBalance.Create(merchantId, date);
        balance.ApplyTransaction(CreateTransaction(merchantId, date, TransactionType.Credit, 200));
        balance.ApplyTransaction(CreateTransaction(merchantId, date, TransactionType.Debit, 50));
        context.DailyBalances.Add(balance);
        await context.SaveChangesAsync();
    }

    private static Transaction CreateTransaction(
        Guid merchantId,
        DateOnly date,
        TransactionType type,
        decimal amount)
    {
        return Transaction.Create(
            merchantId,
            type,
            amount,
            date.ToDateTime(new TimeOnly(10, 0)),
            "Integration test");
    }
}