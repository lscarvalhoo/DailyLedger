using LedgerFlow.Domain.Exceptions;

namespace LedgerFlow.Domain.Entities;

public sealed class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    private User()
    {
    }

    public static User Create(Guid id, string email, string passwordHash, DateTime createdAt)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("User id must be provided.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new DomainException("User email must be provided.");
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new DomainException("User password hash must be provided.");
        }

        return new User
        {
            Id = id,
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            CreatedAt = createdAt
        };
    }
}