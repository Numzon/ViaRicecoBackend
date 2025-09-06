using FluentValidation;
using JetBrains.Annotations;
using ViaRiceco.Common.Application.Abstractions;
using ViaRiceco.Common.Application.Extensions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Accounting.Application.Abstractions.Data;
using ViaRiceco.Modules.Accounting.Application.SettlementPeriods.Models;
using ViaRiceco.Modules.Accounting.Domain.Incomes;
using ViaRiceco.Modules.Accounting.Domain.SettlementPeriods;

namespace ViaRiceco.Modules.Accounting.Application.SettlementPeriods.AddIncome;

public sealed record AddIncomeCommand(string SettlementPeriodId, decimal Value) : ICommand<IncomeDto>;

internal sealed class AddIncomeCommandHandler(
    ISettlementPeriodRepository repository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<AddIncomeCommand, IncomeDto>
{
    public async Task<Result<IncomeDto>> Handle(AddIncomeCommand request, CancellationToken cancellationToken)
    {
        SettlementPeriod? settlementPeriod = await repository.GetAsync(request.SettlementPeriodId, cancellationToken);

        if (settlementPeriod is null)
        {
            return Result.Failure<IncomeDto>(SettlementPeriodErrors.NotFound(request.SettlementPeriodId));
        }

        Income income = settlementPeriod.AddIncome(request.Value, timeProvider.UtcNow());
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var incomeDto = new IncomeDto(income.Id, income.Value, income.CreatedAtUtc, income.UpdatedAtUtc);

        return incomeDto;
    }
}

[UsedImplicitly]
internal sealed class AddIncomeCommandValidator : AbstractValidator<AddIncomeCommand>
{
    public AddIncomeCommandValidator()
    {
        RuleFor(x => x.SettlementPeriodId)
            .NotEmpty()
            .WithMessage("Settlement period ID is required");

        RuleFor(x => x.Value)
            .GreaterThan(0)
            .WithMessage("Income value must be greater than zero");
    }
}
