using System.Data;
using System.Data.Common;

namespace ViaRiceco.Modules.Portfolios.Application.Abstractions.Data;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<DbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}
