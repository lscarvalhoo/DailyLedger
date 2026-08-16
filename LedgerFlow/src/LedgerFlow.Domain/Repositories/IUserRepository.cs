using LedgerFlow.Domain.Entities;

namespace LedgerFlow.Domain.Repositories;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(
        string normalizedEmail,
        CancellationToken cancellationToken = default);
}