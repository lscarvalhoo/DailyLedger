using LedgerFlow.Application.DTOs;
using LedgerFlow.Domain.Entities;

namespace LedgerFlow.Application.Abstractions.Authentication;

public interface IJwtTokenProvider
{
    AuthenticationDto Create(User user);
}