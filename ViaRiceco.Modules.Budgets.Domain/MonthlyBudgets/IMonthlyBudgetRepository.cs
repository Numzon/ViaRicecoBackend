namespace ViaRiceco.Modules.Budgets.Domain.MonthlyBudgets;

public interface IMonthlyBudgetRepository
{
    Task<MonthlyBudget?> GetAsync(string id, CancellationToken cancellationToken = default);
    Task<MonthlyBudget?> GetBySettlementPeriodIdAsync(string settlementPeriodId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<MonthlyBudget>> GetPageAsync(string? search, string orderBy, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<MonthlyBudget>> GetByMonthAndYearAsync(int month, int year, CancellationToken cancellationToken = default);
    Task<bool> ExistsForSettlementPeriodAsync(string settlementPeriodId, CancellationToken cancellationToken = default);
    Task<int> CountAsync(string? search, CancellationToken cancellationToken = default);
    void Insert(MonthlyBudget monthlyBudget);
    void Delete(MonthlyBudget monthlyBudget);
}
