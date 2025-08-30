using System.Dynamic;
using FluentValidation;
using JetBrains.Annotations;
using ViaRiceco.Common.Application.Abstractions;
using ViaRiceco.Common.Application.Extensions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Accounting.Application.Abstractions.Data;
using ViaRiceco.Modules.Accounting.Application.TaxTypes.Models;
using ViaRiceco.Modules.Accounting.Domain.TaxTypes;

namespace ViaRiceco.Modules.Accounting.Application.TaxTypes.CreateTaxType;

public sealed record CreateTaxTypeCommand(string Name) : ICommand<TaxTypeDto>;

internal sealed class CreateTaxTypeCommandHandler(
    ITaxTypeRepository repository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<CreateTaxTypeCommand, TaxTypeDto>
{
    public async Task<Result<TaxTypeDto>> Handle(CreateTaxTypeCommand request, CancellationToken cancellationToken)
    {
        bool exists = await repository.ExistsByNameAsync(request.Name, cancellationToken);
        if (exists)
        {
            return Result.Failure<TaxTypeDto>(TaxTypeErrors.NameNotUnique(request.Name));
        }

        var taxType = TaxType.Create(request.Name, timeProvider.UtcNow());

        repository.Insert(taxType);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var taxTypeDto = new TaxTypeDto(taxType.Id, taxType.Name);

        return taxTypeDto;
    }
}

[UsedImplicitly]
internal sealed class CreateTaxTypeCommandValidator : AbstractValidator<CreateTaxTypeCommand>
{
    public CreateTaxTypeCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
    }
}
