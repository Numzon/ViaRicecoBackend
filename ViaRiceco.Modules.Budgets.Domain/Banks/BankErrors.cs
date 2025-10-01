using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Budgets.Domain.Banks;

public static class BankErrors
{
    public static Error NotFound(string bankId) => Error.NotFound(
        "Bank.NotFound", 
        $"Bank with Id '{bankId}' was not found.");

    public static Error DuplicateName(string name) => Error.Conflict(
        "Bank.DuplicateName", 
        $"Bank with name '{name}' already exists.");

    public static Error InUseByExpenses(string bankId) => Error.Conflict(
        "Bank.InUseByExpenses",
        $"Bank with ID '{bankId}' cannot be deleted because it is referenced by one or more expenses.");

    public static Error InvalidSortParameter(string? sort) => Error.Validation(
        "Bank.InvalidSortParameter", 
        $"Invalid sort parameter: '{sort}'");
}
