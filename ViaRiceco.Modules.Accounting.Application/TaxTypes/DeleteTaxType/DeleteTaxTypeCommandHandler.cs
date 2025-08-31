using FluentValidation;
using JetBrains.Annotations;
using ViaRiceco.Common.Application.Abstractions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Accounting.Application.Abstractions.Data;
using ViaRiceco.Modules.Accounting.Domain.TaxTypes;

namespace ViaRiceco.Modules.Accounting.Application.TaxTypes.DeleteTaxType;

public sealed record DeleteTaxTypeCommand(string Id) : ICommand;

internal sealed class DeleteTaxTypeCommandHandler(
    ITaxTypeRepository repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteTaxTypeCommand>
{
    public async Task<Result> Handle(DeleteTaxTypeCommand request, CancellationToken cancellationToken)
    {
        TaxType? taxType = await repository.GetAsync(request.Id, cancellationToken);
        if (taxType is null)
        {
            return Result.Failure(TaxTypeErrors.NotFound(request.Id));
        }

        repository.Delete(taxType);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

[UsedImplicitly]
internal sealed class DeleteTaxTypeCommandValidator : AbstractValidator<DeleteTaxTypeCommand>
{
    public DeleteTaxTypeCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
