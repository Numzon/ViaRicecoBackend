using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ViaRiceco.Modules.Portfolios.Domain.Investments;
using ViaRiceco.Modules.Portfolios.Domain.PurchaseRecords;

namespace ViaRiceco.Modules.Portfolios.Infrastructure.Investments;

internal sealed class InvestmentConfiguration : IEntityTypeConfiguration<Investment>
{
    public void Configure(EntityTypeBuilder<Investment> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(i => i.Name)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(i => i.InvestmentStrategyId)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(i => i.ModelPortfolioPercentage)
               .IsRequired()
               .HasPrecision(5, 2); // 0.00 to 100.00

        builder.Property(i => i.RealPortfolioPercentage)
               .IsRequired()
               .HasPrecision(5, 2); // 0.00 to 100.00

        builder.Property(i => i.CurrentAmount)
               .IsRequired()
               .HasPrecision(18, 2);

        builder.Property(i => i.CreatedAtUtc)
               .IsRequired();

        builder.Property(i => i.UpdatedAtUtc);

        // Configure navigation properties for collections (not owned)
        builder.HasMany<PurchaseRecord>()
               .WithOne()
               .HasForeignKey(pr => pr.InvestmentId)
               .OnDelete(DeleteBehavior.Cascade);

        // Index on strategy for efficient queries
        builder.HasIndex(i => i.InvestmentStrategyId);

        // Unique constraint on name within strategy
        builder.HasIndex(i => new { i.Name, i.InvestmentStrategyId })
               .IsUnique();
    }
}
