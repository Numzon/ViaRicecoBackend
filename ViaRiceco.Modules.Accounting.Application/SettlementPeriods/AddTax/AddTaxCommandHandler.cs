using FluentValidation;
using JetBrains.Annotations;
using ViaRiceco.Common.Application.Abstractions;
using ViaRiceco.Common.Application.Extensions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Accounting.Application.Abstractions.Data;
using ViaRiceco.Modules.Accounting.Application.SettlementPeriods.Models;
using ViaRiceco.Modules.Accounting.Domain.SettlementPeriods;
using ViaRiceco.Modules.Accounting.Domain.Taxes;
using ViaRiceco.Modules.Accounting.Domain.TaxTypes;

namespace ViaRiceco.Modules.Accounting.Application.SettlementPeriods.AddTax;

public sealed record AddTaxCommand(string SettlementPeriodId, decimal Value, string TaxTypeId) : ICommand<TaxDto>;

internal sealed class AddTaxCommandHandler(
    ISettlementPeriodRepository settlementPeriodRepository,
    ITaxTypeRepository taxTypeRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<AddTaxCommand, TaxDto>
{
    public async Task<Result<TaxDto>> Handle(AddTaxCommand request, CancellationToken cancellationToken)
    {
        SettlementPeriod? settlementPeriod = await settlementPeriodRepository.GetAsync(request.SettlementPeriodId, cancellationToken);
        if (settlementPeriod is null)
        {
            return Result.Failure<TaxDto>(SettlementPeriodErrors.NotFound(request.SettlementPeriodId));
        }

        // Verify tax type exists
        TaxType? taxType = await taxTypeRepository.GetAsync(request.TaxTypeId, cancellationToken);
        if (taxType is null)
        {
            return Result.Failure<TaxDto>(TaxTypeErrors.NotFound(request.TaxTypeId));
        }

        Result result = settlementPeriod.AddTax(request.Value, request.TaxTypeId, timeProvider.UtcNow());
        if (!result.IsSuccess)
        {
            return Result.Failure<TaxDto>(result.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Get the most recently added tax
        Tax addedTax = settlementPeriod.Taxes.OrderByDescending(t => t.CreatedAtUtc).First();
        var taxDto = new TaxDto(addedTax.Id, addedTax.Value, addedTax.TaxTypeId, addedTax.CreatedAtUtc, addedTax.UpdatedAtUtc);

        return taxDto;
    }
}

[UsedImplicitly]
internal sealed class AddTaxCommandValidator : AbstractValidator<AddTaxCommand>
{
    public AddTaxCommandValidator()
    {
        RuleFor(x => x.SettlementPeriodId)
            .NotEmpty()
            .WithMessage("Settlement period ID is required");

        RuleFor(x => x.TaxTypeId)
            .NotEmpty()
            .WithMessage("Tax type ID is required");

        RuleFor(x => x.Value)
            .GreaterThan(0)
            .WithMessage("Tax value must be greater than zero");
    }
}
