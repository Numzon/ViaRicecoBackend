using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ViaRiceco.Modules.Portfolios.Domain.Investments;
using ViaRiceco.Modules.Portfolios.Domain.InvestmentStrategies;

namespace ViaRiceco.Modules.Portfolios.Infrastructure.InvestmentStrategies;

internal sealed class InvestmentStrategyConfiguration : IEntityTypeConfiguration<InvestmentStrategy>
{
    public void Configure(EntityTypeBuilder<InvestmentStrategy> builder)
    {
        builder.HasKey(strategy => strategy.Id);

        builder.Property(strategy => strategy.Id)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(strategy => strategy.FinancialGoalId)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(strategy => strategy.InvestmentStrategyTypeId)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(strategy => strategy.UninvestedAmount)
               .IsRequired()
               .HasPrecision(18, 2);

        builder.Property(strategy => strategy.CreatedAtUtc)
               .IsRequired();

        builder.Property(strategy => strategy.UpdatedAtUtc);

        // Configure navigation properties for collections (not owned)
        builder.HasMany<Investment>()
               .WithOne()
               .HasForeignKey(i => i.InvestmentStrategyId)
               .OnDelete(DeleteBehavior.Cascade);

        // Index on financial goal for efficient queries
        builder.HasIndex(strategy => strategy.FinancialGoalId);

        // Index on strategy type for efficient queries
        builder.HasIndex(strategy => strategy.InvestmentStrategyTypeId);
    }
}
