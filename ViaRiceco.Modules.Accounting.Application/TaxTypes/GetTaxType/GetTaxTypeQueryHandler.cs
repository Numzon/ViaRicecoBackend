using FluentValidation;
using JetBrains.Annotations;
using ViaRiceco.Common.Application.Abstractions.Queries;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Accounting.Application.TaxTypes.Models;
using ViaRiceco.Modules.Accounting.Domain.TaxTypes;

namespace ViaRiceco.Modules.Accounting.Application.TaxTypes.GetTaxType;

public sealed record GetTaxTypeQuery(string Id) : IQuery<TaxTypeDto>;

internal sealed class GetTaxTypeQueryHandler(ITaxTypeRepository repository)
    : IQueryHandler<GetTaxTypeQuery, TaxTypeDto>
{
    public async Task<Result<TaxTypeDto>> Handle(GetTaxTypeQuery request, CancellationToken cancellationToken)
    {
        TaxType? taxType = await repository.GetAsync(request.Id, cancellationToken);
        if (taxType is null)
        {
            return Result.Failure<TaxTypeDto>(TaxTypeErrors.NotFound(request.Id));
        }

        var taxTypeDto = new TaxTypeDto(taxType.Id, taxType.Name);

        return taxTypeDto;
    }
}

[UsedImplicitly]
internal sealed class GetTaxTypeQueryValidator : AbstractValidator<GetTaxTypeQuery>
{
    public GetTaxTypeQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}