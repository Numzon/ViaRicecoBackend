using ViaRiceco.Common.Application.Abstractions;
using ViaRiceco.Common.Application.Extensions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Accounting.Application.Abstractions.Data;
using ViaRiceco.Modules.Accounting.Domain.SettlementPeriods;

namespace ViaRiceco.Modules.Accounting.Application.SettlementPeriods.DeleteSettlementPeriod;

public sealed record DeleteSettlementPeriodCommand(string Id) : ICommand;

internal sealed class DeleteSettlementPeriodCommandHandler(
    ISettlementPeriodRepository repository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<DeleteSettlementPeriodCommand>
{
    public async Task<Result> Handle(DeleteSettlementPeriodCommand request, CancellationToken cancellationToken)
    {
        SettlementPeriod? settlementPeriod = await repository.GetAsync(request.Id, cancellationToken);

        if (settlementPeriod is null)
        {
            return Result.Failure(SettlementPeriodErrors.NotFound(request.Id));
        }

        if (SettlementPeriodSpecification.HasFinancialData(settlementPeriod))
        {
            return Result.Failure(SettlementPeriodErrors.CannotDeleteWithData());
        }

        settlementPeriod.PrepareForDeletion(timeProvider.UtcNow());

        repository.Delete(settlementPeriod);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
