using FluentValidation;
using JetBrains.Annotations;
using ViaRiceco.Common.Application.Abstractions;
using ViaRiceco.Common.Application.Extensions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Budgets.Application.Abstractions.Data;
using ViaRiceco.Modules.Budgets.Application.Banks.Models;
using ViaRiceco.Modules.Budgets.Domain.Banks;

namespace ViaRiceco.Modules.Budgets.Application.Banks.CreateBank;

public sealed record CreateBankCommand(string Name) : ICommand<BankDto>;

internal sealed class CreateBankCommandHandler(
    IBankRepository bankRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<CreateBankCommand, BankDto>
{
    public async Task<Result<BankDto>> Handle(CreateBankCommand request, CancellationToken cancellationToken)
    {
        bool exists = await bankRepository.ExistsByNameAsync(request.Name, cancellationToken);
        if (exists)
        {
            return Result.Failure<BankDto>(BankErrors.DuplicateName(request.Name));
        }

        var bank = Bank.Create(request.Name, DateTime.SpecifyKind(timeProvider.GetUtcNow().DateTime, DateTimeKind.Utc));
        
        bankRepository.Insert(bank);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var bankDto = new BankDto(bank.Id, bank.Name);

        return bankDto;
    }
}

[UsedImplicitly]
internal sealed class CreateBankCommandValidator : AbstractValidator<CreateBankCommand>
{
    public CreateBankCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Bank name is required")
            .MaximumLength(200)
            .WithMessage("Bank name cannot exceed 200 characters");
    }
}
