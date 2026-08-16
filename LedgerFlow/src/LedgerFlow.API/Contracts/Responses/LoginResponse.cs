using LedgerFlow.Application.DTOs;

namespace LedgerFlow.API.Contracts.Responses;

public sealed record LoginResponse(
    string AccessToken,
    string TokenType,
    DateTime ExpiresAt)
{
    public static LoginResponse From(AuthenticationDto authentication)
    {
        return new LoginResponse(
            authentication.AccessToken,
            "Bearer",
            authentication.ExpiresAt);
    }
}