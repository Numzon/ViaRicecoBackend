using FluentValidation;
using JetBrains.Annotations;
using ViaRiceco.Common.Application.Abstractions.Queries;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Budgets.Application.Expenses.Models;
using ViaRiceco.Modules.Budgets.Domain.Expenses;

namespace ViaRiceco.Modules.Budgets.Application.Expenses.GetExpense;

public sealed record GetExpenseQuery(string Id) : IQuery<ExpenseDto>;

internal sealed class GetExpenseQueryHandler(IExpenseRepository repository)
    : IQueryHandler<GetExpenseQuery, ExpenseDto>
{
    public async Task<Result<ExpenseDto>> Handle(GetExpenseQuery request, CancellationToken cancellationToken)
    {
        Expense? expense = await repository.GetAsync(request.Id, cancellationToken);
        if (expense is null)
        {
            return Result.Failure<ExpenseDto>(ExpenseErrors.NotFound(request.Id));
        }

        var expenseDto = new ExpenseDto(expense.Id, expense.Name, expense.ExpenseTypeId, expense.BankId);

        return expenseDto;
    }
}

[UsedImplicitly]
internal sealed class GetExpenseQueryValidator : AbstractValidator<GetExpenseQuery>
{
    public GetExpenseQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Expense ID is required");
    }
}
