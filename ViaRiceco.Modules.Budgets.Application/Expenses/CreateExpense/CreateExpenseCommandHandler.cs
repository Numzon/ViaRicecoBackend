using FluentValidation;
using JetBrains.Annotations;
using ViaRiceco.Common.Application.Abstractions;
using ViaRiceco.Common.Application.Extensions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Budgets.Application.Abstractions.Data;
using ViaRiceco.Modules.Budgets.Application.Expenses.Models;
using ViaRiceco.Modules.Budgets.Domain.Expenses;
using ViaRiceco.Modules.Budgets.Domain.ExpenseTypes;
using ViaRiceco.Modules.Budgets.Domain.Banks;

namespace ViaRiceco.Modules.Budgets.Application.Expenses.CreateExpense;

public sealed record CreateExpenseCommand(string Name, string ExpenseTypeId, string? BankId) : ICommand<ExpenseDto>;

internal sealed class CreateExpenseCommandHandler(
    IExpenseRepository expenseRepository,
    IExpenseTypeRepository expenseTypeRepository,
    IBankRepository bankRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<CreateExpenseCommand, ExpenseDto>
{
    public async Task<Result<ExpenseDto>> Handle(CreateExpenseCommand request, CancellationToken cancellationToken)
    {
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
        
        bool exists = await expenseRepository.ExistsByNameAndExpenseTypeAsync(request.Name, request.ExpenseTypeId, cancellationToken);
        if (exists)
        {
            return Result.Failure<ExpenseDto>(ExpenseErrors.DuplicateNameInExpenseType(request.Name, request.ExpenseTypeId));
        }

        var expense = Expense.Create(request.Name, request.ExpenseTypeId, request.BankId, DateTime.SpecifyKind(timeProvider.UtcNow(), DateTimeKind.Utc));
        
        expenseRepository.Insert(expense);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var expenseDto = new ExpenseDto(expense.Id, expense.Name, expense.ExpenseTypeId, expense.BankId);

        return expenseDto;
    }
}

[UsedImplicitly]
internal sealed class CreateExpenseCommandValidator : AbstractValidator<CreateExpenseCommand>
{
    public CreateExpenseCommandValidator()
    {
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
