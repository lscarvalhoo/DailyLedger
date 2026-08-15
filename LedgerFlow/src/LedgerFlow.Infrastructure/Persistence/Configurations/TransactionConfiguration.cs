using LedgerFlow.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LedgerFlow.Infrastructure.Persistence.Configurations;

public sealed class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transactions");

        builder.HasKey(transaction => transaction.Id);

        builder.Property(transaction => transaction.MerchantId)
            .IsRequired();

        builder.Property(transaction => transaction.Type)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(transaction => transaction.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(transaction => transaction.OccurredAt)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(transaction => transaction.Description)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(transaction => transaction.CreatedAt)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Ignore(transaction => transaction.DomainEvents);

        builder.HasIndex(transaction => new { transaction.MerchantId, transaction.OccurredAt });
    }
}