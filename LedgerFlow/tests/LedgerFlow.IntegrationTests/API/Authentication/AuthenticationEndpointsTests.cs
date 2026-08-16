using LedgerFlow.API.Contracts.Responses;
using LedgerFlow.IntegrationTests.Common;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace LedgerFlow.IntegrationTests.API.Authentication;

[Collection(LedgerFlowApiCollection.Name)]
public sealed class AuthenticationEndpointsTests(LedgerFlowApiFactory factory)
{
    [Fact]
    public async Task Login_WhenCredentialsAreValid_ShouldReturnJwt()
    {
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "usuarioteste@roxpartner.com",
            password = "TesteRoxpartner!"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();
        Assert.True(body?.Success);
        Assert.Equal("Bearer", body?.Data?.TokenType);
        Assert.False(string.IsNullOrWhiteSpace(body?.Data?.AccessToken));
    }

    [Fact]
    public async Task Login_WhenPasswordIsInvalid_ShouldReturnUnauthorized()
    {
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "usuarioteste@roxpartner.com",
            password = "wrong-password"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("Invalid email or password.", document.RootElement.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task Login_WhenEmailIsInvalid_ShouldReturnValidationError()
    {
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "invalid-email",
            password = "TesteRoxpartner!"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Transactions_WhenTokenIsMissing_ShouldReturnUnauthorized()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync($"/api/transactions/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}