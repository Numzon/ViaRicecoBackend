using FluentValidation;
using JetBrains.Annotations;
using ViaRiceco.Common.Application.Abstractions;
using ViaRiceco.Common.Application.Extensions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Budgets.Application.Abstractions.Data;
using ViaRiceco.Modules.Budgets.Application.ExpenseTypes.Models;
using ViaRiceco.Modules.Budgets.Domain.ExpenseTypes;

namespace ViaRiceco.Modules.Budgets.Application.ExpenseTypes.UpdateExpenseType;

public sealed record UpdateExpenseTypeCommand(string Id, string Name) : ICommand<ExpenseTypeDto>;

internal sealed class UpdateExpenseTypeCommandHandler(
    IExpenseTypeRepository repository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<UpdateExpenseTypeCommand, ExpenseTypeDto>
{
    public async Task<Result<ExpenseTypeDto>> Handle(UpdateExpenseTypeCommand request, CancellationToken cancellationToken)
    {
        ExpenseType? expenseType = await repository.GetAsync(request.Id, cancellationToken);
        if (expenseType is null)
        {
            return Result.Failure<ExpenseTypeDto>(ExpenseTypeErrors.NotFound(request.Id));
        }

        bool nameExists = await repository.ExistsByNameAsync(request.Name, request.Id, cancellationToken);
        if (nameExists)
        {
            return Result.Failure<ExpenseTypeDto>(ExpenseTypeErrors.DuplicateName(request.Name));
        }

        Result updateResult = expenseType.Update(request.Name, timeProvider.GetUtcNow().DateTime);
        
        if (updateResult.IsFailure)
        {
            return Result.Failure<ExpenseTypeDto>(updateResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var expenseTypeDto = new ExpenseTypeDto(expenseType.Id, expenseType.Name, expenseType.IsSystemDefined);

        return expenseTypeDto;
    }
}

[UsedImplicitly]
internal sealed class UpdateExpenseTypeCommandValidator : AbstractValidator<UpdateExpenseTypeCommand>
{
    public UpdateExpenseTypeCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Expense type ID is required");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Expense type name is required")
            .MaximumLength(200)
            .WithMessage("Expense type name cannot exceed 200 characters");
    }
}
