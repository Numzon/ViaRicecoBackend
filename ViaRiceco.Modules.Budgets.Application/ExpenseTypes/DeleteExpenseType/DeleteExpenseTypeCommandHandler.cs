using FluentValidation;
using JetBrains.Annotations;
using ViaRiceco.Common.Application.Abstractions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Budgets.Application.Abstractions.Data;
using ViaRiceco.Modules.Budgets.Application.Expenses.Models;
using ViaRiceco.Modules.Budgets.Domain.Expenses;
using ViaRiceco.Modules.Budgets.Domain.ExpenseTypes;

namespace ViaRiceco.Modules.Budgets.Application.ExpenseTypes.DeleteExpenseType;

public sealed record DeleteExpenseTypeCommand(string Id) : ICommand;

internal sealed class DeleteExpenseTypeCommandHandler(
    IExpenseTypeRepository repository,
    IExpenseRepository expenseRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteExpenseTypeCommand>
{
    public async Task<Result> Handle(DeleteExpenseTypeCommand request, CancellationToken cancellationToken)
    {
        ExpenseType? expenseType = await repository.GetAsync(request.Id, cancellationToken);
        if (expenseType is null)
        {
            return Result.Failure(ExpenseTypeErrors.NotFound(request.Id));
        }

        // Prevent deletion of system-defined expense types
        if (!ExpenseTypeSpecification.CanBeDeleted(expenseType))
        {
            return Result.Failure(ExpenseTypeErrors.CannotUpdateSystemDefined());
        }
        
        // Check if expense type is in use by any expenses
        bool isExpenseTypeInUse = await expenseRepository.ExistsByExpenseTypeAsync(
            request.Id, 
            cancellationToken);
        
        if (isExpenseTypeInUse)
        {
            return Result.Failure(ExpenseTypeErrors.InUse(request.Id));
        }

        repository.Delete(expenseType);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

[UsedImplicitly]
internal sealed class DeleteExpenseTypeCommandValidator : AbstractValidator<DeleteExpenseTypeCommand>
{
    public DeleteExpenseTypeCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Expense type ID is required");
    }
}
