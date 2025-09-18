namespace ViaRiceco.Modules.Budgets.Domain.Expenses;

public interface IExpenseRepository
{
    Task<Expense?> GetAsync(string id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Expense>> GetByExpenseTypeAsync(string expenseTypeId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Expense>> GetPageAsync(string? search, string orderBy, int page, int pageSize,
        string? expenseTypeId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAndExpenseTypeAsync(string name, string expenseTypeId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAndExpenseTypeAsync(string name, string expenseTypeId, string excludeId, CancellationToken cancellationToken = default);
    Task<int> CountAsync(string? search, string? expenseTypeId, CancellationToken cancellationToken = default);
    void Insert(Expense expense);
    void Delete(Expense expense);
}

