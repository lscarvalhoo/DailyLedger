using LedgerFlow.IntegrationTests.Common;
using System.Net;

namespace LedgerFlow.IntegrationTests.API.Health;

[Collection(LedgerFlowApiCollection.Name)]
public sealed class HealthEndpointsTests(LedgerFlowApiFactory factory)
{
    [Fact]
    public async Task GetHealth_ShouldReturnOk()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetSwaggerDocument_ShouldReturnOpenApiJson()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }
}