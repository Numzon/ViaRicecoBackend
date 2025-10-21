using FluentValidation;
using JetBrains.Annotations;
using ViaRiceco.Common.Application.Abstractions;
using ViaRiceco.Common.Application.Extensions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Budgets.Application.Abstractions.Data;
using ViaRiceco.Modules.Budgets.Application.ExpenseTypes.Models;
using ViaRiceco.Modules.Budgets.Domain.ExpenseTypes;

namespace ViaRiceco.Modules.Budgets.Application.ExpenseTypes.CreateExpenseType;

public sealed record CreateExpenseTypeCommand(string Name) : ICommand<ExpenseTypeDto>;

internal sealed class CreateExpenseTypeCommandHandler(
    IExpenseTypeRepository repository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<CreateExpenseTypeCommand, ExpenseTypeDto>
{
    public async Task<Result<ExpenseTypeDto>> Handle(CreateExpenseTypeCommand request, CancellationToken cancellationToken)
    {
        bool exists = await repository.ExistsByNameAsync(request.Name, cancellationToken);
        if (exists)
        {
            return Result.Failure<ExpenseTypeDto>(ExpenseTypeErrors.DuplicateName(request.Name));
        }

        var expenseType = ExpenseType.Create(request.Name, timeProvider.UtcNow());
        
        repository.Insert(expenseType);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var expenseTypeDto = new ExpenseTypeDto(expenseType.Id, expenseType.Name, expenseType.IsSystemDefined);

        return expenseTypeDto;
    }
}

[UsedImplicitly]
internal sealed class CreateExpenseTypeCommandValidator : AbstractValidator<CreateExpenseTypeCommand>
{
    public CreateExpenseTypeCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Expense type name is required")
            .MaximumLength(200)
            .WithMessage("Expense type name cannot exceed 200 characters");
    }
}
