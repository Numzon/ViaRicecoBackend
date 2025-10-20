using FluentValidation;
using JetBrains.Annotations;
using ViaRiceco.Common.Application.Abstractions;
using ViaRiceco.Common.Application.Extensions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Accounting.Application.Abstractions.Data;
using ViaRiceco.Modules.Accounting.Application.SettlementPeriods.Models;
using ViaRiceco.Modules.Accounting.Domain.SettlementPeriods;
using ViaRiceco.Modules.Accounting.Domain.Taxes;

namespace ViaRiceco.Modules.Accounting.Application.Taxes.UpdateTax;

public sealed record UpdateTaxCommand(string SettlementPeriodId, string TaxId, decimal Value) : ICommand<TaxDto>;

internal sealed class UpdateTaxCommandHandler(
    ISettlementPeriodRepository repository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<UpdateTaxCommand, TaxDto>
{
    public async Task<Result<TaxDto>> Handle(UpdateTaxCommand request, CancellationToken cancellationToken)
    {
        SettlementPeriod? settlementPeriod = await repository.GetAsync(request.SettlementPeriodId, cancellationToken);

        if (settlementPeriod is null)
        {
            return Result.Failure<TaxDto>(SettlementPeriodErrors.NotFound(request.SettlementPeriodId));
        }

        Result<Tax> result = settlementPeriod.UpdateTax(request.TaxId, request.Value, timeProvider.UtcNow());
        if (!result.IsSuccess)
        {
            return Result.Failure<TaxDto>(result.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        Tax tax = result.Value;
        var taxDto = new TaxDto(tax.Id, tax.Value, tax.TaxTypeId, tax.CreatedAtUtc, tax.UpdatedAtUtc);

        return taxDto;
    }
}

[UsedImplicitly]
internal sealed class UpdateTaxCommandValidator : AbstractValidator<UpdateTaxCommand>
{
    public UpdateTaxCommandValidator()
    {
        RuleFor(x => x.SettlementPeriodId)
            .NotEmpty()
            .WithMessage("Settlement period ID is required");

        RuleFor(x => x.TaxId)
            .NotEmpty()
            .WithMessage("Tax ID is required");

        RuleFor(x => x.Value)
            .GreaterThan(0)
            .WithMessage("Tax value must be greater than zero");
    }
}

