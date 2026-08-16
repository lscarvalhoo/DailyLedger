using LedgerFlow.Domain.Entities;
using LedgerFlow.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LedgerFlow.Infrastructure.Persistence;

public static class DatabaseSeedExtensions
{
    private static readonly Guid TestUserId = Guid.Parse("7b387e6f-9f5a-47eb-bf9f-1bc458ee0c65");
    private const string TestUserEmail = "usuarioteste@roxpartner.com";
    private const string TestUserPassword = "TesteRoxpartner!";

    public static async Task SeedDefaultUserAsync(
        this IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<LedgerFlowDbContext>();

        if (await context.Users.AnyAsync(user => user.Email == TestUserEmail, cancellationToken))
        {
            return;
        }

        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
        var hashTarget = User.Create(TestUserId, TestUserEmail, "pending", DateTime.UtcNow);
        var passwordHash = passwordHasher.HashPassword(hashTarget, TestUserPassword);
        var user = User.Create(TestUserId, TestUserEmail, passwordHash, DateTime.UtcNow);

        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken);
    }
}