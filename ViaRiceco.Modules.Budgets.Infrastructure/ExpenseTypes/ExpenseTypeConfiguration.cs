using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ViaRiceco.Modules.Budgets.Domain.ExpenseTypes;

namespace ViaRiceco.Modules.Budgets.Infrastructure.ExpenseTypes;

internal sealed class ExpenseTypeConfiguration : IEntityTypeConfiguration<ExpenseType>
{
    public void Configure(EntityTypeBuilder<ExpenseType> builder)
    {
        builder.HasKey(et => et.Id);

        builder.Property(et => et.Id)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(et => et.Name)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(et => et.IsSystemDefined)
               .IsRequired()
               .HasDefaultValue(false); // Default to user-defined

        builder.Property(et => et.CreatedAtUtc)
               .IsRequired();

        builder.Property(et => et.UpdatedAtUtc);

        // Unique constraint on expense type name
        builder.HasIndex(et => et.Name)
               .IsUnique();

        // Seed default expense type
        builder.HasData(new
        {
            Id = $"et_{Guid.Parse("db610449-8a5f-47d0-be6a-ec26e4945375")}", // Fixed ID for seeding
            Name = ExpenseTypeSpecification.DefaultExpenseTypeName,
            IsSystemDefined = true, // Mark as system-defined
            CreatedAtUtc = new DateTime(2025, 9, 18, 18, 37, 0, DateTimeKind.Utc),
            UpdatedAtUtc = (DateTime?)null
        });
    }
}
