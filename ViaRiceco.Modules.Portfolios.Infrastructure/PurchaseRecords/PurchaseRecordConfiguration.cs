using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ViaRiceco.Modules.Portfolios.Domain.PurchaseRecords;

namespace ViaRiceco.Modules.Portfolios.Infrastructure.PurchaseRecords;

internal sealed class PurchaseRecordConfiguration : IEntityTypeConfiguration<PurchaseRecord>
{
    public void Configure(EntityTypeBuilder<PurchaseRecord> builder)
    {
        builder.HasKey(pr => pr.Id);

        builder.Property(pr => pr.Id)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(pr => pr.PurchaseDate)
               .IsRequired();

        builder.Property(pr => pr.Amount)
               .IsRequired()
               .HasPrecision(18, 8); // High precision for investment amounts

        builder.Property(pr => pr.PricePerUnit)
               .IsRequired()
               .HasPrecision(18, 8); // High precision for unit prices

        builder.Property(pr => pr.TotalPrice)
               .IsRequired()
               .HasPrecision(18, 2); // Standard currency precision

        builder.Property(pr => pr.CurrencyId)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(pr => pr.InvestmentId)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(pr => pr.CreatedAtUtc)
               .IsRequired();

        builder.Property(pr => pr.UpdatedAtUtc);

        // Index on investment for efficient queries
        builder.HasIndex(pr => pr.InvestmentId);

        // Index on currency for efficient queries
        builder.HasIndex(pr => pr.CurrencyId);

        // Index on purchase date for date range queries
        builder.HasIndex(pr => pr.PurchaseDate);
    }
}
