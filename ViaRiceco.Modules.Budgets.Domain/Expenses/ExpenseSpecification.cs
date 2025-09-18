namespace ViaRiceco.Modules.Budgets.Domain.Expenses;

public static class ExpenseSpecification
{
    /// <summary>
    /// Determines whether the expense name is valid (not null or empty)
    /// </summary>
    public static bool IsValidName(string? name)
    {
        return !string.IsNullOrWhiteSpace(name);
    }

    /// <summary>
    /// Determines whether the expense type ID is valid (not null or empty)
    /// </summary>
    public static bool IsValidExpenseTypeId(string? expenseTypeId)
    {
        return !string.IsNullOrWhiteSpace(expenseTypeId);
    }

    /// <summary>
    /// Determines whether the provided ID is valid for integration events (not null or empty)
    /// </summary>
    public static bool IsValidId(string? id)
    {
        return !string.IsNullOrWhiteSpace(id);
    }

    /// <summary>
    /// Determines whether all expense creation parameters are valid
    /// </summary>
    public static bool AreCreateParametersValid(string? name, string? expenseTypeId)
    {
        return IsValidName(name) && IsValidExpenseTypeId(expenseTypeId);
    }

    /// <summary>
    /// Determines whether all expense update parameters are valid
    /// </summary>
    public static bool AreUpdateParametersValid(string? name, string? expenseTypeId)
    {
        return IsValidName(name) && IsValidExpenseTypeId(expenseTypeId);
    }
}
