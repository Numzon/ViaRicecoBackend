namespace ViaRiceco.Modules.Accounting.Domain.SettlementPeriods;

public interface ISettlementPeriodRepository
{
    Task<SettlementPeriod?> GetAsync(string id, CancellationToken cancellationToken = default);
    Task<SettlementPeriod?> GetByMonthAndYearAsync(int month, int year, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<SettlementPeriod>> GetPageAsync(string? search, string orderBy, int page, int pageSize,
        int? month, int? year, CancellationToken cancellationToken = default);
    Task<bool> ExistsByMonthAndYearAsync(int month, int year, CancellationToken cancellationToken = default);
    Task<bool> ExistsByMonthAndYearAsync(int month, int year, string excludeId, CancellationToken cancellationToken = default);
    Task<int> CountAsync(string? search, int? month, int? year, CancellationToken cancellationToken = default);
    void Insert(SettlementPeriod settlementPeriod);
    void Delete(SettlementPeriod settlementPeriod);
}
