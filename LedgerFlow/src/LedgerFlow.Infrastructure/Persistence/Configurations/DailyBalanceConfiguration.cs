using LedgerFlow.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LedgerFlow.Infrastructure.Persistence.Configurations;

public sealed class DailyBalanceConfiguration : IEntityTypeConfiguration<DailyBalance>
{
    public void Configure(EntityTypeBuilder<DailyBalance> builder)
    {
        builder.ToTable("DailyBalances");

        builder.HasKey(dailyBalance => dailyBalance.Id);

        builder.Property(dailyBalance => dailyBalance.MerchantId)
            .IsRequired();

        builder.Property(dailyBalance => dailyBalance.Date)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(dailyBalance => dailyBalance.TotalCredits)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(dailyBalance => dailyBalance.TotalDebits)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(dailyBalance => dailyBalance.Balance)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(dailyBalance => dailyBalance.UpdatedAt)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.HasIndex(dailyBalance => new { dailyBalance.MerchantId, dailyBalance.Date })
            .IsUnique();
    }
}