using FluentValidation;
using JetBrains.Annotations;
using ViaRiceco.Common.Application.Abstractions;
using ViaRiceco.Common.Application.Extensions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Accounting.Application.Abstractions.Data;
using ViaRiceco.Modules.Accounting.Application.SettlementPeriods.Models;
using ViaRiceco.Modules.Accounting.Domain.SettlementPeriods;

namespace ViaRiceco.Modules.Accounting.Application.SettlementPeriods.CreateSettlementPeriod;

public sealed record CreateSettlementPeriodCommand(int Month, int Year) : ICommand<SettlementPeriodDto>;

internal sealed class CreateSettlementPeriodCommandHandler(
    ISettlementPeriodRepository repository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<CreateSettlementPeriodCommand, SettlementPeriodDto>
{
    public async Task<Result<SettlementPeriodDto>> Handle(CreateSettlementPeriodCommand request,
        CancellationToken cancellationToken)
    {
        bool exists = await repository.ExistsByMonthAndYearAsync(request.Month, request.Year, cancellationToken);
        if (exists)
        {
            return Result.Failure<SettlementPeriodDto>(
                SettlementPeriodErrors.AlreadyExists(request.Month, request.Year));
        }

        bool hasDraftPeriod = await repository.HasDraftPeriodAsync(cancellationToken);
        if (hasDraftPeriod)
        {
            return Result.Failure<SettlementPeriodDto>(SettlementPeriodErrors.DraftPeriodExists());
        }

        var settlementPeriod = SettlementPeriod.Create(request.Month, request.Year, timeProvider.UtcNow());

        repository.Insert(settlementPeriod);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        IReadOnlyCollection<IncomeDto> incomes =
            [.. settlementPeriod.Incomes.Select(i => new IncomeDto(i.Id, i.Value, i.CreatedAtUtc, i.UpdatedAtUtc))];

        IReadOnlyCollection<TaxDto> taxes =
        [
            .. settlementPeriod.Taxes.Select(t =>
                new TaxDto(t.Id, t.Value, t.TaxTypeId, t.CreatedAtUtc, t.UpdatedAtUtc))
        ];

        var settlementPeriodDto = new SettlementPeriodDto(
            settlementPeriod.Id,
            settlementPeriod.Month,
            settlementPeriod.Year,
            settlementPeriod.IsDraft,
            settlementPeriod.TotalIncome,
            settlementPeriod.TotalTaxes,
            settlementPeriod.NetAmount,
            settlementPeriod.CreatedAtUtc,
            settlementPeriod.UpdatedAtUtc,
            incomes,
            taxes);

        return settlementPeriodDto;
    }
}

[UsedImplicitly]
internal sealed class CreateSettlementPeriodCommandValidator : AbstractValidator<CreateSettlementPeriodCommand>
{
    public CreateSettlementPeriodCommandValidator()
    {
        RuleFor(x => x.Month)
            .InclusiveBetween(1, 12)
            .WithMessage("Month must be between 1 and 12");

        RuleFor(x => x.Year)
            .InclusiveBetween(1900, 2100)
            .WithMessage("Year must be between 1900 and 2100");
    }
}
