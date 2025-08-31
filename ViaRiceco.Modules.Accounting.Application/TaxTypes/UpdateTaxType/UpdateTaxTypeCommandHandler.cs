using FluentValidation;
using JetBrains.Annotations;
using ViaRiceco.Common.Application.Abstractions;
using ViaRiceco.Common.Application.Extensions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Accounting.Application.Abstractions.Data;
using ViaRiceco.Modules.Accounting.Application.TaxTypes.Models;
using ViaRiceco.Modules.Accounting.Domain.TaxTypes;

namespace ViaRiceco.Modules.Accounting.Application.TaxTypes.UpdateTaxType;

public sealed record UpdateTaxTypeCommand(string Id, string Name) : ICommand<TaxTypeDto>;

internal sealed class UpdateTaxTypeCommandHandler(
    ITaxTypeRepository repository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<UpdateTaxTypeCommand, TaxTypeDto>
{
    public async Task<Result<TaxTypeDto>> Handle(UpdateTaxTypeCommand request, CancellationToken cancellationToken)
    {
        TaxType? taxType = await repository.GetAsync(request.Id, cancellationToken);
        if (taxType is null)
        {
            return Result.Failure<TaxTypeDto>(TaxTypeErrors.NotFound(request.Id));
        }

        bool nameExists = await repository.ExistsByNameAsync(request.Name, request.Id, cancellationToken);
        if (nameExists)
        {
            return Result.Failure<TaxTypeDto>(TaxTypeErrors.NameNotUnique(request.Name));
        }

        taxType.Update(request.Name, timeProvider.UtcNow());

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var taxTypeDto = new TaxTypeDto(taxType.Id, taxType.Name);

        return taxTypeDto;
    }
}

[UsedImplicitly]
internal sealed class UpdateTaxTypeCommandValidator : AbstractValidator<UpdateTaxTypeCommand>
{
    public UpdateTaxTypeCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty();
    }
}
