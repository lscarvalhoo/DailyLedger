namespace LedgerFlow.Application.DTOs;

public sealed record AuthenticationDto(
    string AccessToken,
    DateTime ExpiresAt);