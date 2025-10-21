using FluentValidation;
using JetBrains.Annotations;
using ViaRiceco.Common.Application.Abstractions.Queries;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Budgets.Application.MonthlyBudgets.Models;
using ViaRiceco.Modules.Budgets.Application.MonthlyBudgetExpenses.Models;
using ViaRiceco.Modules.Budgets.Domain.MonthlyBudgets;

namespace ViaRiceco.Modules.Budgets.Application.MonthlyBudgets.GetMonthlyBudget;

public sealed record GetMonthlyBudgetQuery(string Id) : IQuery<MonthlyBudgetDto>;

internal sealed class GetMonthlyBudgetQueryHandler(IMonthlyBudgetRepository repository) : IQueryHandler<GetMonthlyBudgetQuery, MonthlyBudgetDto>
{
    public async Task<Result<MonthlyBudgetDto>> Handle(GetMonthlyBudgetQuery request, CancellationToken cancellationToken)
    {
        MonthlyBudget? monthlyBudget = await repository.GetAsync(request.Id, cancellationToken);
        if (monthlyBudget is null)
        {
            return Result.Failure<MonthlyBudgetDto>(MonthlyBudgetErrors.NotFound(request.Id));
        }

        IReadOnlyCollection<MonthlyBudgetExpenseDto> expenses =
            monthlyBudget.Expenses.Select(e =>
                new MonthlyBudgetExpenseDto(
                    e.Id,
                    e.MonthlyBudgetId,
                    e.ExpenseId,
                    e.ExpenseName,
                    e.ExpenseTypeId,
                    e.ExpenseTypeName,
                    e.Value,
                    e.CreatedAtUtc,
                    e.UpdatedAtUtc)).ToList();

        var monthlyBudgetDto = new MonthlyBudgetDto(
            monthlyBudget.Id,
            monthlyBudget.SettlementPeriodId,
            monthlyBudget.Month,
            monthlyBudget.Year,
            monthlyBudget.IsDraft,
            monthlyBudget.NetValue,
            expenses,
            monthlyBudget.TotalBudgetedAmount,
            monthlyBudget.CreatedAtUtc,
            monthlyBudget.UpdatedAtUtc);

        return monthlyBudgetDto;
    }
}

[UsedImplicitly]
internal sealed class GetMonthlyBudgetQueryValidator : AbstractValidator<GetMonthlyBudgetQuery>
{
    public GetMonthlyBudgetQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Monthly budget ID is required");
    }
}
