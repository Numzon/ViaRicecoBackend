using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ViaRiceco.Modules.Budgets.Domain.Banks;

namespace ViaRiceco.Modules.Budgets.Infrastructure.Banks;

internal sealed class BankConfiguration : IEntityTypeConfiguration<Bank>
{
    public void Configure(EntityTypeBuilder<Bank> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
               .HasMaxLength(50)
               .ValueGeneratedNever();

        builder.Property(b => b.Name)
               .HasMaxLength(200)
               .IsRequired();

        builder.Property(b => b.CreatedAtUtc)
               .IsRequired();

        builder.Property(b => b.UpdatedAtUtc);

        // Indexes
        builder.HasIndex(b => b.Name)
               .IsUnique();

        builder.HasIndex(b => b.CreatedAtUtc);
    }
}
