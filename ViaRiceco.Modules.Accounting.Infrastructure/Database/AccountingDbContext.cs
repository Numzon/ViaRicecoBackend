using Microsoft.EntityFrameworkCore;
using ViaRiceco.Modules.Accounting.Application.Abstractions.Data;
using ViaRiceco.Modules.Accounting.Domain.SettlementPeriods;
using ViaRiceco.Modules.Accounting.Domain.TaxTypes;
using ViaRiceco.Modules.Accounting.Infrastructure.SettlementPeriods;
using ViaRiceco.Modules.Accounting.Infrastructure.TaxTypes;

namespace ViaRiceco.Modules.Accounting.Infrastructure.Database;

public sealed class AccountingDbContext(DbContextOptions<AccountingDbContext> options)
    : DbContext(options), IUnitOfWork
{
    internal DbSet<TaxType> TaxTypes { get; set; }
    internal DbSet<SettlementPeriod> SettlementPeriods { get; set; }
        
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Accounting);

        modelBuilder.ApplyConfiguration(new TaxTypeConfiguration());
        modelBuilder.ApplyConfiguration(new SettlementPeriodConfiguration());
    }
}
