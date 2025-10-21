using ViaRiceco.Common.Application.Abstractions;

namespace ViaRiceco.Modules.Budgets.Application.MonthlyBudgets.DeleteMonthlyBudget;

public sealed record DeleteMonthlyBudgetIntegrationEventCommand(string SettlementPeriodId) : ICommand;
