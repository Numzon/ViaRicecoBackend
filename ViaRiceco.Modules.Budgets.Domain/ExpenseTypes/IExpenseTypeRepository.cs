namespace ViaRiceco.Modules.Budgets.Domain.ExpenseTypes;

public interface IExpenseTypeRepository
{
    Task<ExpenseType?> GetAsync(string id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ExpenseType>> GetPageAsync(string? search, string orderBy, int page, int pageSize,
        CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, string excludeId, CancellationToken cancellationToken = default);
    Task<int> CountAsync(string? search, CancellationToken cancellationToken = default);
    void Insert(ExpenseType expenseType);
    void Delete(ExpenseType expenseType);
}
