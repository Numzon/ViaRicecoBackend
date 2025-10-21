using FluentValidation;
using JetBrains.Annotations;
using ViaRiceco.Common.Application.Abstractions;
using ViaRiceco.Common.Application.Extensions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Accounting.Application.Abstractions.Data;
using ViaRiceco.Modules.Accounting.Domain.SettlementPeriods;

namespace ViaRiceco.Modules.Accounting.Application.SettlementPeriods.FinalizeSettlementPeriod;

public sealed record FinalizeSettlementPeriodCommand(string Id) : ICommand;

internal sealed class FinalizeSettlementPeriodCommandHandler(
    ISettlementPeriodRepository repository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<FinalizeSettlementPeriodCommand>
{
    public async Task<Result> Handle(FinalizeSettlementPeriodCommand request,
        CancellationToken cancellationToken)
    {
        SettlementPeriod? settlementPeriod = await repository.GetAsync(request.Id, cancellationToken);

        if (settlementPeriod is null)
        {
            return Result.Failure(SettlementPeriodErrors.NotFound(request.Id));
        }

        Result result = settlementPeriod.Finalize(timeProvider.UtcNow());
        if (!result.IsSuccess)
        {
            return Result.Failure(result.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

[UsedImplicitly]
internal sealed class FinalizeSettlementPeriodCommandValidator : AbstractValidator<FinalizeSettlementPeriodCommand>
{
    public FinalizeSettlementPeriodCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Settlement period ID is required");
    }
}
