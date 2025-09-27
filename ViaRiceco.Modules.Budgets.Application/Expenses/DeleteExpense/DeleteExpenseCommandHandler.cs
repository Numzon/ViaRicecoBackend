using FluentValidation;
using JetBrains.Annotations;
using ViaRiceco.Common.Application.Abstractions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Budgets.Application.Abstractions.Data;
using ViaRiceco.Modules.Budgets.Domain.Expenses;

namespace ViaRiceco.Modules.Budgets.Application.Expenses.DeleteExpense;

public sealed record DeleteExpenseCommand(string Id) : ICommand;

internal sealed class DeleteExpenseCommandHandler(
    IExpenseRepository repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteExpenseCommand>
{
    public async Task<Result> Handle(DeleteExpenseCommand request, CancellationToken cancellationToken)
    {
        Expense? expense = await repository.GetAsync(request.Id, cancellationToken);
        if (expense is null)
        {
            return Result.Failure(ExpenseErrors.NotFound(request.Id));
        }

        repository.Delete(expense);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

[UsedImplicitly]
internal sealed class DeleteExpenseCommandValidator : AbstractValidator<DeleteExpenseCommand>
{
    public DeleteExpenseCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Expense ID is required");
    }
}
