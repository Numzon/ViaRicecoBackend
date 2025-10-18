using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Budgets.Domain.MonthlyBudgetExpenses;

namespace ViaRiceco.Modules.Budgets.Domain.MonthlyBudgets;

public sealed class MonthlyBudget : Entity
{
    private readonly List<MonthlyBudgetExpense> _expenses = [];

    private MonthlyBudget()
    {
    }

    public string SettlementPeriodId { get; private set; } = string.Empty;
    public int Month { get; private set; }
    public int Year { get; private set; }
    public bool IsDraft { get; private set; }
    public decimal NetValue { get; private set; }

    public IReadOnlyCollection<MonthlyBudgetExpense> Expenses => _expenses.AsReadOnly();
    
    public decimal TotalBudgetedAmount => _expenses
        .Where(e => e.Value.HasValue)
        .Sum(e => e.Value!.Value);

    public static MonthlyBudget CreateFromSettlementPeriod(
        string settlementPeriodId,
        int month,
        int year,
        decimal netValue,
        IReadOnlyCollection<ExpenseData> activeExpenses,
        DateTime createdAtUtc)
    {
        var monthlyBudget = new MonthlyBudget
        {
            Id = $"mb_{Guid.NewGuid()}",
            SettlementPeriodId = settlementPeriodId,
            Month = month,
            Year = year,
            IsDraft = true, 
            NetValue = netValue,
            CreatedAtUtc = createdAtUtc
        };

        foreach (ExpenseData expenseData in activeExpenses)
        {
            var budgetExpense = MonthlyBudgetExpense.Create(
                monthlyBudget.Id,
                expenseData.ExpenseId,
                expenseData.ExpenseName,
                expenseData.ExpenseTypeId,
                expenseData.ExpenseTypeName,
                createdAtUtc);

            monthlyBudget._expenses.Add(budgetExpense);
        }

        monthlyBudget.Raise(new MonthlyBudgetCreatedDomainEvent(
            monthlyBudget.Id,
            settlementPeriodId,
            month,
            year,
            activeExpenses.Count,
            createdAtUtc));

        return monthlyBudget;
    }

    public Result Finalize(DateTime finalizedAtUtc)
    {
        if (!IsDraft)
        {
            return Result.Failure(MonthlyBudgetErrors.AlreadyFinalized());
        }

        var expensesWithoutValues = _expenses.Where(e => !e.Value.HasValue).ToList();
        if (expensesWithoutValues.Any())
        {
            return Result.Failure(MonthlyBudgetErrors.CannotFinalizeWithNullValues(expensesWithoutValues.Count));
        }

        IsDraft = false;
        UpdatedAtUtc = finalizedAtUtc;

        Raise(new MonthlyBudgetFinalizedDomainEvent(Id, TotalBudgetedAmount, finalizedAtUtc));

        return Result.Success();
    }

    public Result SetAsDraft(DateTime updatedAtUtc)
    {
        if (IsDraft)
        {
            return Result.Failure(MonthlyBudgetErrors.AlreadyDraft());
        }

        IsDraft = true;
        UpdatedAtUtc = updatedAtUtc;

        Raise(new MonthlyBudgetSetAsDraftDomainEvent(Id, updatedAtUtc));
        
        return Result.Success();
    }

    public Result AddExpense(
        ExpenseData expenseData,
        DateTime addedAtUtc)
    {
        if (!IsDraft)
        {
            return Result.Failure(MonthlyBudgetErrors.CannotModifyFinalizedBudget());
        }

        if (_expenses.Any(e => e.ExpenseId == expenseData.ExpenseId))
        {
            return Result.Failure(MonthlyBudgetErrors.ExpenseAlreadyExists(expenseData.ExpenseId));
        }

        var budgetExpense = MonthlyBudgetExpense.Create(
            Id,
            expenseData.ExpenseId,
            expenseData.ExpenseName,
            expenseData.ExpenseTypeId,
            expenseData.ExpenseTypeName,
            addedAtUtc);

        _expenses.Add(budgetExpense);
        UpdatedAtUtc = addedAtUtc;

        Raise(new ExpenseAddedToMonthlyBudgetDomainEvent(Id, expenseData.ExpenseId, expenseData.ExpenseName, addedAtUtc));

        return Result.Success();
    }

    public Result UpdateExpenseName(string expenseId, string newName, DateTime updatedAtUtc)
    {
        if (!IsDraft)
        {
            return Result.Success(); 
        }

        MonthlyBudgetExpense? expense = _expenses.Find(e => e.ExpenseId == expenseId);
        if (expense == null)
        {
            return Result.Failure(MonthlyBudgetErrors.ExpenseNotFound(expenseId));
        }

        expense.UpdateExpenseName(newName, updatedAtUtc);
        UpdatedAtUtc = updatedAtUtc;

        return Result.Success();
    }

    public Result RemoveExpense(string monthlyBudgetExpenseId, DateTime removedAtUtc)
    {
        if (!IsDraft)
        {
            return Result.Success(); 
        }

        MonthlyBudgetExpense? expense = _expenses.Find(e => e.Id == monthlyBudgetExpenseId);
        if (expense == null)
        {
            return Result.Failure(MonthlyBudgetErrors.ExpenseNotFound(monthlyBudgetExpenseId));
        }

        _expenses.Remove(expense);
        UpdatedAtUtc = removedAtUtc;

        Raise(new ExpenseRemovedFromMonthlyBudgetDomainEvent(Id, monthlyBudgetExpenseId, removedAtUtc));

        return Result.Success();
    }

    public Result SetExpenseValue(string expenseId, decimal? value, DateTime updatedAtUtc)
    {
        MonthlyBudgetExpense? expense = _expenses.Find(e => e.ExpenseId == expenseId);
        if (expense == null)
        {
            return Result.Failure(MonthlyBudgetErrors.ExpenseNotFound(expenseId));
        }

        expense.SetValue(value, updatedAtUtc);
        UpdatedAtUtc = updatedAtUtc;

        return Result.Success();
    }

    public Result BulkSetExpenseValues(IReadOnlyCollection<ExpenseValueUpdate> expenseValueUpdates, DateTime updatedAtUtc)
    {
        bool hasChanges = false;
        var changedExpenseUpdates = new List<ExpenseValueUpdate>();

        foreach (ExpenseValueUpdate update in expenseValueUpdates)
        {
            MonthlyBudgetExpense? expense = _expenses.Find(e => e.Id == update.MonthlyBudgetExpenseId);
            if (expense == null)
            {
                return Result.Failure(MonthlyBudgetErrors.ExpenseNotFound(update.MonthlyBudgetExpenseId));
            }

            if (expense.Value != update.Value)
            {
                expense.SetValue(update.Value, updatedAtUtc);
                changedExpenseUpdates.Add(update);
                hasChanges = true;
            }
        }

        if (hasChanges)
        {
            UpdatedAtUtc = updatedAtUtc;
            Raise(new MonthlyBudgetExpenseValuesBulkSetDomainEvent(Id, changedExpenseUpdates, updatedAtUtc));
        }

        return Result.Success();
    }

    public void UpdateNetValue(decimal netValue, DateTime updatedAtUtc)
    {
        NetValue = netValue;
        UpdatedAtUtc = updatedAtUtc;

        Raise(new MonthlyBudgetNetValueUpdatedDomainEvent(Id, netValue, updatedAtUtc));
    }
}
