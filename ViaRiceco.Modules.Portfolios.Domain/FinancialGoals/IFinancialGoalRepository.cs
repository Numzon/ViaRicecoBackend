namespace ViaRiceco.Modules.Portfolios.Domain.FinancialGoals;

public interface IFinancialGoalRepository
{
    Task<FinancialGoal?> GetAsync(string id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<FinancialGoal>> GetRootGoalsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<FinancialGoal>> GetChildrenAsync(string parentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<FinancialGoal>> GetPageAsync(string? search, string orderBy, int page, int pageSize,
        CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, string excludeId, CancellationToken cancellationToken = default);
    Task<bool> HasChildrenAsync(string goalId, CancellationToken cancellationToken = default);
    Task<int> CountAsync(string? search, CancellationToken cancellationToken = default);
    void Insert(FinancialGoal financialGoal);
    void Delete(FinancialGoal financialGoal);
}
