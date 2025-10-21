using ViaRiceco.Common.Application.Abstractions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Budgets.Application.Abstractions.Data;
using ViaRiceco.Modules.Budgets.Domain.Banks;

namespace ViaRiceco.Modules.Budgets.Application.Banks.DeleteBank;

public sealed record DeleteBankCommand(string Id) : ICommand;

internal sealed class DeleteBankCommandHandler(
    IBankRepository bankRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteBankCommand>
{
    public async Task<Result> Handle(DeleteBankCommand request, CancellationToken cancellationToken)
    {
        Bank? bank = await bankRepository.GetAsync(request.Id, cancellationToken);
        if (bank is null)
        {
            return Result.Success(); // Idempotent - already deleted
        }

        bool isInUse = await bankRepository.IsInUseAsync(request.Id, cancellationToken);
        if (isInUse)
        {
            return Result.Failure(BankErrors.InUse(request.Id));
        }

        bankRepository.Delete(bank);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
