using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Portfolios.Domain.PurchaseRecords;

public interface IPurchaseRecordRepository
{
    Task<PurchaseRecord?> GetAsync(string id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<PurchaseRecord>> GetByInvestmentAsync(string investmentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<PurchaseRecord>> GetByCurrencyAsync(string currencyId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<PurchaseRecord>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<PurchaseRecord>> GetPageAsync(BaseQueryParameters queryParameters, DateRangeParameters dateRange, string? investmentId, string? currencyId, CancellationToken cancellationToken = default);
    Task<int> CountAsync(BaseQueryParameters queryParameters, DateRangeParameters dateRange, string? investmentId, string? currencyId, CancellationToken cancellationToken = default);
    void Insert(PurchaseRecord purchaseRecord);
    void Delete(PurchaseRecord purchaseRecord);
}
