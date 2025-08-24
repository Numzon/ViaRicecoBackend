using Microsoft.EntityFrameworkCore;
using ViaRiceco.Modules.Accounting.Domain.TaxTypes;
using ViaRiceco.Modules.Accounting.Infrastructure.Database;

namespace ViaRiceco.Modules.Accounting.Infrastructure.TaxTypes;

internal sealed class TaxTypeRepository(AccountingDbContext context) : ITaxTypeRepository
{
    public Task<TaxType?> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        return context.TaxTypes.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public void Insert(TaxType taxType)
    {
        context.TaxTypes.Add(taxType);
    }
}
