using FluentValidation;
using JetBrains.Annotations;
using ViaRiceco.Common.Application.Abstractions;
using ViaRiceco.Common.Application.Extensions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Accounting.Application.Abstractions.Data;
using ViaRiceco.Modules.Accounting.Application.SettlementPeriods.Models;
using ViaRiceco.Modules.Accounting.Domain.Incomes;
using ViaRiceco.Modules.Accounting.Domain.SettlementPeriods;

namespace ViaRiceco.Modules.Accounting.Application.Incomes.UpdateIncome;

public sealed record UpdateIncomeCommand(string SettlementPeriodId, string IncomeId, decimal Value) : ICommand<IncomeDto>;

internal sealed class UpdateIncomeCommandHandler(
    ISettlementPeriodRepository repository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<UpdateIncomeCommand, IncomeDto>
{
    public async Task<Result<IncomeDto>> Handle(UpdateIncomeCommand request, CancellationToken cancellationToken)
    {
        SettlementPeriod? settlementPeriod = await repository.GetAsync(request.SettlementPeriodId, cancellationToken);

        if (settlementPeriod is null)
        {
            return Result.Failure<IncomeDto>(SettlementPeriodErrors.NotFound(request.SettlementPeriodId));
        }

        Result<Income> result = settlementPeriod.UpdateIncome(request.IncomeId, request.Value, timeProvider.UtcNow());
        if (!result.IsSuccess)
        {
            return Result.Failure<IncomeDto>(result.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        Income income = result.Value;
        var incomeDto = new IncomeDto(income.Id, income.Value, income.CreatedAtUtc, income.UpdatedAtUtc);

        return incomeDto;
    }
}

[UsedImplicitly]
internal sealed class UpdateIncomeCommandValidator : AbstractValidator<UpdateIncomeCommand>
{
    public UpdateIncomeCommandValidator()
    {
        RuleFor(x => x.SettlementPeriodId)
            .NotEmpty()
            .WithMessage("Settlement period ID is required");

        RuleFor(x => x.IncomeId)
            .NotEmpty()
            .WithMessage("Income ID is required");

        RuleFor(x => x.Value)
            .GreaterThan(0)
            .WithMessage("Income value must be greater than zero");
    }
}

