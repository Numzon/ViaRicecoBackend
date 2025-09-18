namespace ViaRiceco.Modules.Budgets.Domain.ExpenseTypes;

public static class ExpenseTypeSpecification
{
    public const string DefaultExpenseTypeName = "Investment";

    /// <summary>
    /// Determines whether the expense type name is valid (not null or empty)
    /// </summary>
    public static bool IsValidName(string? name)
    {
        return !string.IsNullOrWhiteSpace(name);
    }

    /// <summary>
    /// Determines whether this is the default expense type
    /// </summary>
    public static bool IsDefaultExpenseType(ExpenseType expenseType)
    {
        return string.Equals(expenseType.Name, DefaultExpenseTypeName, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines whether the given name represents the default expense type
    /// </summary>
    public static bool IsDefaultExpenseTypeName(string? name)
    {
        return string.Equals(name, DefaultExpenseTypeName, StringComparison.OrdinalIgnoreCase);
    }
}
