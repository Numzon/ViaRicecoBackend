using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ViaRiceco.Modules.Portfolios.Domain.InvestmentTypes;

namespace ViaRiceco.Modules.Portfolios.Infrastructure.InvestmentTypes;

internal sealed class InvestmentStrategyTypeConfiguration : IEntityTypeConfiguration<InvestmentStrategyType>
{
    public void Configure(EntityTypeBuilder<InvestmentStrategyType> builder)
    {
        builder.HasKey(ist => ist.Id);

        builder.Property(ist => ist.Id)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(ist => ist.Name)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(ist => ist.CreatedAtUtc)
               .IsRequired();

        builder.Property(ist => ist.UpdatedAtUtc);

        // Unique constraint on name
        builder.HasIndex(ist => ist.Name)
               .IsUnique();
    }
}
