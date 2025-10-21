namespace ViaRiceco.Modules.Budgets.Application.Expenses.Models;

public sealed record ExpenseDto(string Id, string Name, string ExpenseTypeId, string? BankId);
