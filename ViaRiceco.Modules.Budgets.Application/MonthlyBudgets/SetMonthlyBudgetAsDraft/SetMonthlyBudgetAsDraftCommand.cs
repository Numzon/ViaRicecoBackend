using ViaRiceco.Common.Application.Abstractions;

namespace ViaRiceco.Modules.Budgets.Application.MonthlyBudgets.SetMonthlyBudgetAsDraft;

public sealed record SetMonthlyBudgetAsDraftCommand(string Id) : ICommand;
