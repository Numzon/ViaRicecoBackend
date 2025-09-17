using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ViaRiceco.Modules.Portfolios.Domain.Currencies;

namespace ViaRiceco.Modules.Portfolios.Infrastructure.Currencies;

internal sealed class CurrencyConfiguration : IEntityTypeConfiguration<Currency>
{
    public void Configure(EntityTypeBuilder<Currency> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(c => c.Name)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(c => c.Code)
               .IsRequired()
               .HasMaxLength(3);

        builder.Property(c => c.CreatedAtUtc)
               .IsRequired();

        builder.Property(c => c.UpdatedAtUtc);

        // Unique constraint on currency code
        builder.HasIndex(c => c.Code)
               .IsUnique();
    }
}
