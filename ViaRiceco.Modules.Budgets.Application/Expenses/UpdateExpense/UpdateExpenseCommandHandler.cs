using FluentValidation;
using JetBrains.Annotations;
using ViaRiceco.Common.Application.Abstractions;
using ViaRiceco.Common.Application.Extensions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Budgets.Application.Abstractions.Data;
using ViaRiceco.Modules.Budgets.Application.Expenses.Models;
using ViaRiceco.Modules.Budgets.Domain.Banks;
using ViaRiceco.Modules.Budgets.Domain.Expenses;
using ViaRiceco.Modules.Budgets.Domain.ExpenseTypes;

namespace ViaRiceco.Modules.Budgets.Application.Expenses.UpdateExpense;

public sealed record UpdateExpenseCommand(string Id, string Name, string ExpenseTypeId, string? BankId) : ICommand<ExpenseDto>;

internal sealed class UpdateExpenseCommandHandler(
    IExpenseRepository expenseRepository,
    IExpenseTypeRepository expenseTypeRepository,
    IBankRepository bankRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<UpdateExpenseCommand, ExpenseDto>
{
    public async Task<Result<ExpenseDto>> Handle(UpdateExpenseCommand request, CancellationToken cancellationToken)
    {
        Expense? expense = await expenseRepository.GetAsync(request.Id, cancellationToken);
        if (expense is null)
        {
            return Result.Failure<ExpenseDto>(ExpenseErrors.NotFound(request.Id));
        }

        // Verify that the expense type exists
        ExpenseType? expenseType = await expenseTypeRepository.GetAsync(request.ExpenseTypeId, cancellationToken);
        if (expenseType is null)
        {
            return Result.Failure<ExpenseDto>(ExpenseTypeErrors.NotFound(request.ExpenseTypeId));
        }

        // Validate bank exists if specified
        if (!string.IsNullOrEmpty(request.BankId))
        {
            Bank? bank = await bankRepository.GetAsync(request.BankId, cancellationToken);
            if (bank is null)
            {
                return Result.Failure<ExpenseDto>(BankErrors.NotFound(request.BankId));
            }
        }

        // Check for duplicate name within the same expense type (excluding current expense)
        bool nameExists = await expenseRepository.ExistsByNameAndExpenseTypeAsync(request.Name, request.ExpenseTypeId, request.Id, cancellationToken);
        if (nameExists)
        {
            return Result.Failure<ExpenseDto>(ExpenseErrors.DuplicateNameInExpenseType(request.Name, request.ExpenseTypeId));
        }

        expense.Update(request.Name, request.ExpenseTypeId, request.BankId, timeProvider.UtcNow());

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var expenseDto = new ExpenseDto(expense.Id, expense.Name, expense.ExpenseTypeId, expense.BankId);

        return expenseDto;
    }
}

[UsedImplicitly]
internal sealed class UpdateExpenseCommandValidator : AbstractValidator<UpdateExpenseCommand>
{
    public UpdateExpenseCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Expense ID is required");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Expense name is required")
            .MaximumLength(200)
            .WithMessage("Expense name cannot exceed 200 characters");

        RuleFor(x => x.ExpenseTypeId)
            .NotEmpty()
            .WithMessage("Expense type ID is required");
    }
}
