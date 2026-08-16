using LedgerFlow.Domain.Entities;

namespace LedgerFlow.Application.Abstractions.Authentication;

public interface IPasswordVerifier
{
    bool Verify(User user, string password);
}