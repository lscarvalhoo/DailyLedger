using LedgerFlow.API.Contracts.Responses;
using LedgerFlow.Infrastructure.Persistence;
using LedgerFlow.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace LedgerFlow.IntegrationTests.Common;

public sealed class LedgerFlowApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"LedgerFlow-IntegrationTests-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Database", "Server=integration-tests;Database=LedgerFlow");
        builder.UseSetting("RabbitMq:HostName", "integration-tests");
        builder.UseSetting("RabbitMq:Port", "5672");
        builder.ConfigureAppConfiguration(configuration =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Database"] = "Server=integration-tests;Database=LedgerFlow",
                ["RabbitMq:HostName"] = "integration-tests",
                ["RabbitMq:Port"] = "5672"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IHostedService>();
            services.RemoveAll<LedgerFlowDbContext>();
            services.RemoveAll<DbContextOptions<LedgerFlowDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<LedgerFlowDbContext>>();

            services.AddDbContext<LedgerFlowDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));
        });
    }

    public async Task ResetDatabaseAsync()
    {
        await using var scope = Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<LedgerFlowDbContext>();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        await Services.SeedDefaultUserAsync();
    }

    public async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "usuarioteste@roxpartner.com",
            password = "TesteRoxpartner!"
        });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            body!.Data!.AccessToken);
        return client;
    }
}