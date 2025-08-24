using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ViaRiceco.Modules.Accounting.Domain.TaxTypes;

namespace ViaRiceco.Modules.Accounting.Infrastructure.TaxTypes;

internal sealed class TaxTypeConfiguration : IEntityTypeConfiguration<TaxType> 
{
    public void Configure(EntityTypeBuilder<TaxType> builder)
    {
        builder.HasKey(t => t.Id);
        
        builder.Property(t => t.Name)
               .IsRequired()
               .HasMaxLength(200);

        builder.HasIndex(t => t.Name).IsUnique();
    }
}
