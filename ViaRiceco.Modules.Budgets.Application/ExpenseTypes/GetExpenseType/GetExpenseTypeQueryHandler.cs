using FluentValidation;
using JetBrains.Annotations;
using ViaRiceco.Common.Application.Abstractions.Queries;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Budgets.Application.ExpenseTypes.Models;
using ViaRiceco.Modules.Budgets.Domain.ExpenseTypes;

namespace ViaRiceco.Modules.Budgets.Application.ExpenseTypes.GetExpenseType;

public sealed record GetExpenseTypeQuery(string Id) : IQuery<ExpenseTypeDto>;

internal sealed class GetExpenseTypeQueryHandler(IExpenseTypeRepository repository)
    : IQueryHandler<GetExpenseTypeQuery, ExpenseTypeDto>
{
    public async Task<Result<ExpenseTypeDto>> Handle(GetExpenseTypeQuery request, CancellationToken cancellationToken)
    {
        ExpenseType? expenseType = await repository.GetAsync(request.Id, cancellationToken);
        if (expenseType is null)
        {
            return Result.Failure<ExpenseTypeDto>(ExpenseTypeErrors.NotFound(request.Id));
        }

        var expenseTypeDto = new ExpenseTypeDto(expenseType.Id, expenseType.Name, expenseType.IsSystemDefined);

        return expenseTypeDto;
    }
}

[UsedImplicitly]
internal sealed class GetExpenseTypeQueryValidator : AbstractValidator<GetExpenseTypeQuery>
{
    public GetExpenseTypeQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Expense type ID is required");
    }
}
