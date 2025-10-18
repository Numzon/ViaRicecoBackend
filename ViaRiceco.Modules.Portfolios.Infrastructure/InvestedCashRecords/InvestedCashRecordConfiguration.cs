using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ViaRiceco.Modules.Portfolios.Domain.InvestedCashRecords;

namespace ViaRiceco.Modules.Portfolios.Infrastructure.InvestedCashRecords;

internal sealed class InvestedCashRecordConfiguration : IEntityTypeConfiguration<InvestedCashRecord>
{
    public void Configure(EntityTypeBuilder<InvestedCashRecord> builder)
    {
        builder.HasKey(ic => ic.Id);

        builder.Property(ic => ic.Id)
               .HasMaxLength(50)
               .ValueGeneratedNever();

        builder.Property(ic => ic.InvestmentStrategyId)
               .HasMaxLength(50)
               .IsRequired();

        builder.Property(ic => ic.MonthlyBudgetExpenseId)
               .HasMaxLength(50)
               .IsRequired();

        builder.Property(ic => ic.Amount)
               .HasColumnType("decimal(18,2)")
               .IsRequired();

        builder.Property(ic => ic.CreatedAtUtc)
               .IsRequired();

        builder.Property(ic => ic.UpdatedAtUtc);

        // Indexes
        builder.HasIndex(ic => ic.InvestmentStrategyId);
        builder.HasIndex(ic => ic.MonthlyBudgetExpenseId);

        // Unique constraint to prevent duplicates
        builder.HasIndex(ic => new { ic.InvestmentStrategyId, ic.MonthlyBudgetExpenseId })
               .IsUnique();
    }
}

