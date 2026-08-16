using LedgerFlow.Domain.Entities;
using LedgerFlow.Domain.Repositories;
using LedgerFlow.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace LedgerFlow.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(LedgerFlowDbContext context) : IUserRepository
{
    public Task<User?> GetByEmailAsync(
        string normalizedEmail,
        CancellationToken cancellationToken = default)
    {
        return context.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(user => user.Email == normalizedEmail, cancellationToken);
    }
}