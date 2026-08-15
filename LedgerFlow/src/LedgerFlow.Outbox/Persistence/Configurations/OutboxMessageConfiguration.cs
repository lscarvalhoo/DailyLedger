using LedgerFlow.Outbox.Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LedgerFlow.Outbox.Persistence.Configurations;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");

        builder.HasKey(message => message.Id);

        builder.Property(message => message.Type)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(message => message.Payload)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.Property(message => message.CreatedAt)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(message => message.ProcessedAt)
            .HasColumnType("datetime2");

        builder.Property(message => message.RetryCount)
            .IsRequired();

        builder.Property(message => message.TraceParent)
            .HasMaxLength(55);

        builder.Property(message => message.TraceState)
            .HasMaxLength(512);

        builder.HasIndex(message => new { message.ProcessedAt, message.RetryCount, message.CreatedAt });
    }
}