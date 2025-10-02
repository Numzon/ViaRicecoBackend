using FluentValidation;
using JetBrains.Annotations;
using ViaRiceco.Common.Application.Abstractions;
using ViaRiceco.Common.Application.Extensions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Budgets.Application.Abstractions.Data;
using ViaRiceco.Modules.Budgets.Application.Banks.Models;
using ViaRiceco.Modules.Budgets.Domain.Banks;

namespace ViaRiceco.Modules.Budgets.Application.Banks.UpdateBank;

public sealed record UpdateBankCommand(string Id, string Name) : ICommand<BankDto>;

internal sealed class UpdateBankCommandHandler(
    IBankRepository bankRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<UpdateBankCommand, BankDto>
{
    public async Task<Result<BankDto>> Handle(UpdateBankCommand request, CancellationToken cancellationToken)
    {
        Bank? bank = await bankRepository.GetAsync(request.Id, cancellationToken);
        if (bank is null)
        {
            return Result.Failure<BankDto>(BankErrors.NotFound(request.Id));
        }

        bool exists = await bankRepository.ExistsByNameAsync(request.Name, request.Id, cancellationToken);
        if (exists)
        {
            return Result.Failure<BankDto>(BankErrors.DuplicateName(request.Name));
        }

        bank.Update(request.Name, DateTime.SpecifyKind(timeProvider.GetUtcNow().DateTime, DateTimeKind.Utc));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var bankDto = new BankDto(bank.Id, bank.Name);

        return bankDto;
    }
}

[UsedImplicitly]
internal sealed class UpdateBankCommandValidator : AbstractValidator<UpdateBankCommand>
{
    public UpdateBankCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Bank ID is required");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Bank name is required")
            .MaximumLength(200)
            .WithMessage("Bank name cannot exceed 200 characters");
    }
}
