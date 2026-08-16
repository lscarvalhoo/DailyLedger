using LedgerFlow.Application.Abstractions;
using LedgerFlow.Application.DTOs;

namespace LedgerFlow.Application.Authentication.Commands.Login;

public sealed record LoginCommand(
    string? Email,
    string? Password) : ICommand<AuthenticationDto>;